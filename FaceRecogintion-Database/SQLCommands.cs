using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Structure;
using FaceRecognition_Database;
using FaceRecognition_Database.Models;
using MySql.Data.MySqlClient;

namespace FaceRecogintion_Database
{
	class SQLCommands
	{
		public static bool InsertStudent(Student newStudent)
		{
			string cmdString = $"insert students values(@id, @surname, @name, @patronymic, @photo)";
			MySqlCommand cmd = new MySqlCommand(cmdString, DBConnection.conn);
			cmd.Parameters.Add("@id", MySqlDbType.Int32);
			cmd.Parameters.Add("@surname", MySqlDbType.VarChar, 50);
			cmd.Parameters.Add("@name", MySqlDbType.VarChar, 50);
			cmd.Parameters.Add("@patronymic", MySqlDbType.VarChar, 50);
			cmd.Parameters.Add("@photo", MySqlDbType.LongBlob);
			cmd.Parameters["@id"].Value = newStudent.ID;
			cmd.Parameters["@surname"].Value = newStudent.Surname;
			cmd.Parameters["@name"].Value = newStudent.Name;
			cmd.Parameters["@patronymic"].Value = newStudent.Patronymic;
			byte[] b = new byte[]{1,2};
			ImageConverter converter = new ImageConverter();
			b = (byte[])converter.ConvertTo(newStudent.Photo.ToBitmap(), typeof(byte[]));
			cmd.Parameters["@photo"].Value = b;
			int RowsAffected = cmd.ExecuteNonQuery();
			if (RowsAffected > 0)
			{
				return true;
			}
			return false;
		}

		public static List<Student> SelectStudents()
		{
			string cmdString = $"select * from students order by id";
			DataTable dt = DBConnection.ExecuteDataSet(cmdString);

			List<Student> students = new List<Student>();
			foreach (DataRow dr in dt.Rows)
			{
				ImageConverter converter = new ImageConverter();
				Bitmap bmp;
				using (var ms = new MemoryStream((byte[])dr["photo"]))
				{
					bmp = new Bitmap(ms);
				}
				Image<Gray,byte> photo = bmp.ToImage<Gray, byte>();
				students.Add(new Student((int)dr["id"],dr["surname"].ToString(), dr["name"].ToString(), 
					dr["patronymic"].ToString(),photo));
			}
			return students;
		}
	}
}
