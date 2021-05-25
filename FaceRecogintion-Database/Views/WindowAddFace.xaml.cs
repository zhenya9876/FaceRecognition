using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
		public Student NewStudent { get; set; }
		public WindowAddFace(Image<Gray, Byte> facePhoto)
		{
			InitializeComponent();
			FacePhoto = facePhoto;
			imgAddPhoto.Source = BitmapToImageSource.Get(FacePhoto.ToBitmap());
		}

		private void BtnAddFace_OnClick(object sender, RoutedEventArgs e)
		{
			if (tbName.Text != "" && tbSurname.Text != "" && tbPatronymic.Text != "")
			{
				NewStudent = new Student(0, tbSurname.Text, tbName.Text, tbPatronymic.Text, FacePhoto);
				this.DialogResult = true;
			}
			else MessageBox.Show("Заполните все поля!");
		}
	}
}
