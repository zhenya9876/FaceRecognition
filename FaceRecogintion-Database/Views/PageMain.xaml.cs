using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using FaceRecogintion_Database;
using FaceRecogintion_Database.Views;
using FaceRecognition_Database.Models;

namespace FaceRecognition_Database.Views
{
	/// <summary>
	/// Логика взаимодействия для PageMain.xaml
	/// </summary>
	public partial class PageMain : Page
	{
		private DBConnection Con;
		public List<Student> StudentsDB;
		private List<Image<Gray, Byte>> FoundFaces;
		private Dictionary<int,double> FoundFacesIDsLikeness;
		private VectorOfMat FacesDB;
		private VectorOfInt FacesDBIDs;
		private Image<Gray, Byte> SelectedFace;
		private string FaceName = "";
		EigenFaceRecognizer Recognizer;
		private CascadeClassifier HaarCascade;
		//private string HaarCascadePath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)+@"\haarcascade_frontalface_default.xml";
		private string HaarCascadePath = "haarcascade_frontalface_default.xml";
		private Image<Bgr, Byte> BgrFrame = null;

		public PageMain(DBConnection con)
		{
			InitializeComponent();
			Con = con;
			//cbWindows.ItemsSource = ScreenCapture.GetWindowsTitles();
			if (!File.Exists(HaarCascadePath))
			{
				string text = "Cannot find Haar cascade data file:\n\n";
				text += HaarCascadePath;
				MessageBoxResult result = MessageBox.Show(text, "Error",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
			HaarCascade = new CascadeClassifier(HaarCascadePath);
			FoundFaces = new List<Image<Gray, byte>>();

		}

		private void BtnFindFace_OnClick(object sender, RoutedEventArgs e)
		{
			FoundFaces.Clear();
			LoadDB();
			Image<Bgr, Byte> windowImage = null;
			windowImage = ScreenCapture.GetDesktopImage().ToImage<Bgr, Byte>();

			//if ((sender as Button).Name == "btnFindFaceDesktop") windowImage = ScreenCapture.GetDesktopImage().ToImage<Bgr, Byte>();
			//else
			//	try
			//	{
			//		if (cbWindows.SelectedItem != null)
			//			windowImage = (ScreenCapture.GetWindowCaptureAsBitmap(cbWindows.SelectedValue.ToString()).ToImage<Bgr, byte>());
			//	}
			//	catch (Exception exception)
			//	{
			//		MessageBox.Show(exception.Message);
			//	}

			if (windowImage != null)
			{
				try
				{//for emgu cv bug
					Image<Gray, byte> grayframe = windowImage.Convert<Gray, byte>();

					System.Drawing.Rectangle[] faces = HaarCascade.DetectMultiScale(grayframe, 1.2, 5, new System.Drawing.Size(50, 50), new System.Drawing.Size(200, 200));

					//detect face
					FaceName = "No face detected";
					foreach (var face in faces)
					{
						//windowImage.Draw(face, new Bgr(255, 255, 0), 2);
						FoundFaces.Add(windowImage.Copy(face).Convert<Gray, byte>());
					}
				}
				catch (Exception ex)
				{
					//todo log
				}
			}

			if (FoundFaces.Count > 0)
			{
				SelectedFace = FoundFaces.First();
				imgFoundPhoto.Source = BitmapToImageSource
					.Get(SelectedFace.ToBitmap());
				UpdateNumberOfFaces(FoundFaces.IndexOf(SelectedFace), FoundFaces.Count);

			}
			FaceRecognition(SelectedFace);
			lblFaceName.Content = FaceName;
		}
		private void UpdateNumberOfFaces(int index, int count)
		{
			lblNumOfFaces.Content = $"{index} из {count}";
		}
		private void FaceRecognition(Image<Gray,Byte> selectedFace)
		{
			if (FoundFaces.Count > 0)
			{
				//Eigen Face Algorithm
				if (StudentsDB.Count == 0)
				{
					FaceName = "DataBase is Empty";
					MessageBox.Show("База данных пуста");
				}
				else
				{
					if(selectedFace != null)
					{
						Image<Gray, byte> recogFace = selectedFace.Resize(100, 100, Inter.Cubic);
						FaceRecognizer.PredictionResult result = Recognizer.Predict(recogFace);
						try
						{
							FaceName = StudentsDB.First(x => x.ID == result.Label && result.Distance < 500).Surname;
						}
						catch (Exception ex)
						{
							//FaceName = $"{StudentsDB.First(x => x.ID == result.Label).Surname} | {result.Distance}";
							FaceName = $"Возможно это: {StudentsDB.First(x => x.ID == result.Label ).Surname}";
							//FaceName = "Not in the Base";
						}
					}
				}
				imgFoundPhoto.Source = BitmapToImageSource.Get(selectedFace.ToBitmap());
			}
			else
			{
				FaceName = "Лица не найдены";
				MessageBox.Show("Лица не найдены");
			}
		}

		public void LoadDB()
		{
			StudentsDB = new List<Student>();
			try
			{
				StudentsDB = SQLCommands.SelectStudents();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			FillDataBaseDP();
			FacesDB = new VectorOfMat(StudentsDB.Count);
			FacesDBIDs = new VectorOfInt(StudentsDB.Count);
			FacesDB.Clear(); FacesDBIDs.Clear();
			foreach (var student in StudentsDB)
			{
				FacesDB.Push(student.Photo.Resize(100,100,Inter.Cubic).Mat);
				FacesDBIDs.Push(new []{student.ID});
			}
			Recognizer = new EigenFaceRecognizer(StudentsDB.Count);

			if (StudentsDB.Count>0)
			Recognizer.Train(FacesDB, FacesDBIDs);
		}

		private void FillDataBaseDP()
		{
			dpDataBase.Children.Clear();
			foreach (Student student in StudentsDB)
			{
				StudentControl sc = new StudentControl(this, student);
				DockPanel.SetDock(sc, Dock.Top);
				dpDataBase.Children.Add(sc);
			}
			
		}

		private void BtnPrevFace_OnClick(object sender, RoutedEventArgs e)
		{
			int index = FoundFaces.IndexOf(SelectedFace);
			int numOfFaces = FoundFaces.Count;
			if (index == 0) index = numOfFaces - 1;
			else index--;
			UpdateNumberOfFaces(index+1, numOfFaces);
			if (FoundFaces.Count != 0)
			{
				SelectedFace = FoundFaces[index];
				imgFoundPhoto.Source = BitmapToImageSource.Get(SelectedFace.ToBitmap());
				FaceRecognition(SelectedFace);
			}
			else
			{
				imgFoundPhoto.Source = null;
			}
			lblFaceName.Content = FaceName;
		}

		private void BtnNextFace_OnClick(object sender, RoutedEventArgs e)
		{
			int index = FoundFaces.IndexOf(SelectedFace);
			int numOfFaces = FoundFaces.Count;
			if (index == numOfFaces-1) index = 0;
			else index++;
			UpdateNumberOfFaces(index+1, numOfFaces);
			if (FoundFaces.Count != 0)
			{
				SelectedFace = FoundFaces[index];
				imgFoundPhoto.Source = BitmapToImageSource.Get(SelectedFace.ToBitmap());
				FaceRecognition(SelectedFace);
			}
			else
			{
				imgFoundPhoto.Source = null;
			}
			lblFaceName.Content = FaceName;
		}

		private void BtnAddFace_OnClick(object sender, RoutedEventArgs e)
		{
			if (FoundFaces.Count > 0)
			{
				WindowAddFace windowAddFace = new WindowAddFace(SelectedFace);
				if (windowAddFace.ShowDialog() == true)
				{
					try
					{
						StudentsDB.First(x => x.Surname == windowAddFace.NewStudent.Surname &&
						                      x.Name == windowAddFace.NewStudent.Name &&
						                      x.Patronymic == windowAddFace.NewStudent.Patronymic);
						MessageBox.Show("Студент с такими ФИО уже есть в базе");
					}
					catch (Exception exception)
					{ //if user is not found
						windowAddFace.NewStudent.ID = GetUniqueId(FacesDBIDs);
						if (SQLCommands.InsertStudent(windowAddFace.NewStudent))
							MessageBox.Show("Лицо добавлено в базу");
						LoadDB();
					}

				}
				else
				{
					MessageBox.Show("Лицо не было добавлено!");
				}
			}
			else
			{
				MessageBox.Show("Не найдено лиц для добавления!");
			}

		}
		private int GetUniqueId(VectorOfInt facesDBIDs)
		{
			List<int> list = new List<int>();
			for (int j = 0; j < facesDBIDs.Size; j++)
			{
				list.Add(facesDBIDs[j]);
			}

			for (int j = 0; j < facesDBIDs.Size + 1; j++)
			{
				if (!list.Contains(j)) return j;
			}
			return 0;
		}
	}
}
