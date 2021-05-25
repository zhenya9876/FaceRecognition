﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using FaceRecogintion_Database;
using FaceRecogintion_Database.Views;
using FaceRecognition_Database.Models;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Math.EC;
using Image = System.Windows.Controls.Image;

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
			cbWindows.ItemsSource = ScreenCapture.GetWindowsTitles();
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
			Image<Bgr, Byte> windowImage;
			if ((sender as Button).Name == "btnFindFaceDesktop") windowImage = ScreenCapture.GetDesktopImage().ToImage<Bgr,Byte>();
			else windowImage = (ScreenCapture.GetWindowCaptureAsBitmap(cbWindows.SelectedValue.ToString()).ToImage<Bgr, byte>());


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
				if (StudentsDB.Count == 0) FaceName = "DataBase is Empty";
				else
				{
					if(selectedFace != null)
					{
						Image<Gray, byte> recogFace = selectedFace.Resize(100, 100, Inter.Cubic);
						FaceRecognizer.PredictionResult result = Recognizer.Predict(recogFace);
						//List<Student> lookAlikes = new List<Student>();
						//foreach (Student student in StudentsDB)
						//{
						//	if(result.Label == )
						//}
						try
						{
							FaceName = StudentsDB.First(x => x.ID == result.Label && result.Distance < 500).Surname;
						}
						catch (Exception ex)
						{
							FaceName = $"{StudentsDB.First(x => x.ID == result.Label ).Surname} | {result.Distance}";
							//FaceName = "Not in the Base";
						}
					}

				}
				imgFoundPhoto.Source = BitmapToImageSource.Get(selectedFace.ToBitmap());
			}
			else
			{
				FaceName = "Please Add Face";
			}
		}

		private void LoadDB()
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
			int i = 0;
			FacesDB = new VectorOfMat(StudentsDB.Count);
			FacesDBIDs = new VectorOfInt(StudentsDB.Count);
			FacesDB.Clear(); FacesDBIDs.Clear();
			foreach (var student in StudentsDB)
			{
				FacesDB.Push(student.Photo.Resize(100,100,Inter.Cubic).Mat);
				FacesDBIDs.Push(new []{student.ID});
			}
			Recognizer = new EigenFaceRecognizer(StudentsDB.Count);
			//debug
			//foreach (Student student in StudentsDB)
			//{
			//	Window wnd = new Window()
			//		{Content = new Image {Source = BitmapToImageSource.Get(student.Photo.ToBitmap())}};
			//	wnd.Show();
			//}

			if (StudentsDB.Count>0)
			Recognizer.Train(FacesDB, FacesDBIDs);
		}

		private void BtnPrevFace_OnClick(object sender, RoutedEventArgs e)
		{
			int index = FoundFaces.IndexOf(SelectedFace);
			int numOfFaces = FoundFaces.Count;
			if (index == 0) index = numOfFaces - 1;
			else index--;
			UpdateNumberOfFaces(index+1, numOfFaces);
			SelectedFace = FoundFaces[index];
			imgFoundPhoto.Source = BitmapToImageSource.Get(SelectedFace.ToBitmap());
			FaceRecognition(SelectedFace);
			lblFaceName.Content = FaceName;
		}

		private void BtnNextFace_OnClick(object sender, RoutedEventArgs e)
		{
			int index = FoundFaces.IndexOf(SelectedFace);
			int numOfFaces = FoundFaces.Count;
			if (index == numOfFaces-1) index = 0;
			else index++;
			UpdateNumberOfFaces(index+1, numOfFaces);
			SelectedFace = FoundFaces[index];
			imgFoundPhoto.Source = BitmapToImageSource.Get(SelectedFace.ToBitmap());
			FaceRecognition(SelectedFace);
			lblFaceName.Content = FaceName;
		}

		private void BtnAddFace_OnClick(object sender, RoutedEventArgs e)
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
					if(SQLCommands.InsertStudent(windowAddFace.NewStudent))
						MessageBox.Show("Лицо добавлено в базу");
					LoadDB();
					//обновить StudentsDB из базы
					//	throw;
				}
			
			}
			else
			{
				MessageBox.Show("Лицо не было добавлено!");
			}

		}
		private int GetUniqueId(VectorOfInt facesDBIDs)
		{
			List<int> list = new List<int>();
			int i = 0;
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
