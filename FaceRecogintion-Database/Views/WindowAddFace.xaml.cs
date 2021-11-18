using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Structure;
using FaceRecognition_Database.Models;
using MessageBox = System.Windows.MessageBox;

namespace FaceRecogintion_Database.Views
{
	/// <summary>
	/// Логика взаимодействия для WindowAddFace.xaml
	/// </summary>
	public partial class WindowAddFace : Window
	{
		private Image<Gray, Byte> FacePhoto;
		private Dictionary<int, string> GroupsDB;
		private int maxFieldLength = 50;
		public Student NewStudent { get; set; }
		public WindowAddFace(Image<Gray, Byte> facePhoto, Dictionary<int, string> groupsDb)
		{
			InitializeComponent();
			FacePhoto = facePhoto;
			GroupsDB = groupsDb;

			cbGroup.ItemsSource = GroupsDB.Values;
			imgAddPhoto.Source = BitmapToImageSource.Get(FacePhoto.ToBitmap());
		}

		private void BtnAddFace_OnClick(object sender, RoutedEventArgs e)
		{
			if (tbName.Text != "" && tbSurname.Text != "" && tbPatronymic.Text != "")
			{
				if (tbName.Text.Length > maxFieldLength || tbSurname.Text.Length > maxFieldLength
				                                 || tbPatronymic.Text.Length > maxFieldLength)
					MessageBox.Show($"Одно из полей привысило максимальную длину поля ({maxFieldLength})");
				else
				{
					NewStudent = new Student(0, tbSurname.Text, tbName.Text, tbPatronymic.Text, FacePhoto, 
						GroupsDB.FirstOrDefault(x => x.Value == cbGroup.SelectedValue.ToString()).Key);
					this.DialogResult = true;
				}
			}
			else MessageBox.Show("Заполните все поля!");
		}
	}
}
