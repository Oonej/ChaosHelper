using System;
using System.IO;
using System.Reflection;

namespace ChaosHelper
{
	public static class Util
	{
		public static void LogError(Exception ex)
		{
			try
			{
				using (StreamWriter writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Asheron's Call\" + Globals.PluginName + " errors.txt", true))
				{
					writer.WriteLine("============================================================================");
					writer.WriteLine(DateTime.Now.ToString());
					writer.WriteLine("Error: " + ex.Message);
					writer.WriteLine("Source: " + ex.Source);
					writer.WriteLine("Stack: " + ex.StackTrace);
					if (ex.InnerException != null)
					{
						writer.WriteLine("Inner: " + ex.InnerException.Message);
						writer.WriteLine("Inner Stack: " + ex.InnerException.StackTrace);
					}
					writer.WriteLine("============================================================================");
					writer.WriteLine("");
					writer.Close();
				}
			}
			catch
			{
			}
		}

        public static string[] GetIni()
        {
            try
            {
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePath = Path.Combine(assemblyFolder, "chaoshelper.ini");

                return File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                WriteToChat(ex.Message);
            }
            return null;
        }

        public static string[] GetConfig(string configname)
        {
            try
            {
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePath = Path.Combine(assemblyFolder, configname);

                return File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                WriteToChat(ex.Message);
            }
            return null;
        }

        public static string[] GetListofConfigs()
        {
            try
            {
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string[] temp = Directory.GetFiles(assemblyFolder, "*.txt");
                for(int i = 0; i < temp.Length; i++)
                {
                    temp[i] = temp[i].Remove(0, assemblyFolder.Length + 1);
                }
                return temp;
            }
            catch (Exception ex)
            {
                WriteToChat(ex.Message);
            }
            return null;
        }

        public static void WriteToChat(string message)
		{
			try
			{
				Globals.Host.Actions.AddChatText("<{" + Globals.PluginName + "}>: " + message, 5);
			}
			catch (Exception ex) { LogError(ex); }
		}

        public static void SaveIni(string command, string config)
        {
            try
            {
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePath = Path.Combine(assemblyFolder, "chaoshelper.ini");

                string[] lines = File.ReadAllLines(filePath);

                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    foreach (string line in lines)
                    {
                        if (line.Contains("sendchatcommand"))
                        {
                            writer.WriteLine("sendchatcommand:" + command);
                        }
                        else if(line.Contains("default config"))
                        {
                            writer.WriteLine("default config:" + config);
                        }
                    }
                }

                WriteToChat("Chat Command Saved to :" + command);
            }
            catch (Exception ex)
            {
                WriteToChat(ex.Message);
            }
        }

        public static string[] LoadLayout(string layout)
        {
            try
            {
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePath = Path.Combine(assemblyFolder, layout);

                return File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                WriteToChat(ex.Message);
            }
            return null;
        }
	}
}
