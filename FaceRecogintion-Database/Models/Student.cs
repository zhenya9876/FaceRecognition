using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Drawing;
using System.Net.Mime;
using Emgu.CV;
using Emgu.CV.Structure;

namespace FaceRecognition_Database.Models
{
	public class Student
	{
		public int ID { get; set; }
		public string Surname { get; set; }
		public string Name { get; set; }
		public string Patronymic { get; set; }
		public Image<Gray, Byte> Photo { get; set; }

		public Student(int id, string surname, string name, string patronymic, Image<Gray, Byte> photo)
		{
			ID = id;
			Surname = surname;
			Name = name;
			Patronymic = patronymic;
			Photo = photo;
		}
	}
}
