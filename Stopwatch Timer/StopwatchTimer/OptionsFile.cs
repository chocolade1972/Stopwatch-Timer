/*----------------------------------------------------------------
 * Module Name  : OptionsFile
 * Description  : Saves and retrievs application options
 * Author       : Danny
 * Date         : 10/02/2010
 * Revision     : 1.00
 * --------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Configuration;


/*
 *  Introduction :
 * 
 *  This module helps in saving application options
 * 
 * 
 *  Typical file could look like this:
 *  user_color=Red
 *  time_left=30
 *  
 * 
 * 
 * 
 * 
 * */

namespace DannyGeneral
{
    class OptionsFile
    {
        /*----------------------------------------
         *   P R I V A T E     V A R I A B L E S 
         * ---------------------------------------*/
        

        /*---------------------------------
         *   P U B L I C   M E T H O D S 
         * -------------------------------*/
        string path_exe;
        string temp_settings_file;
        string temp_settings_dir;
        string Options_File;
        StreamWriter sw;
        StreamReader sr;

		/*----------------------------------------------------------
		 * Function     : OptionsFile
		 * Description  : Constructor
		 * Parameters   : file_name is the name of the file to use
		 * Return       : none
		 * --------------------------------------------------------*/

		///<summary>
		///<para>Enter a path with a filename</para>
		///</summary>
		public OptionsFile(string settingsFileAndPath)
    {
        if (!File.Exists(settingsFileAndPath))
        {
            if (!Directory.Exists(Path.GetDirectoryName(settingsFileAndPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFileAndPath));
            }
            File.Create(settingsFileAndPath).Close();
        }
        path_exe = Path.GetDirectoryName(Application.LocalUserAppDataPath);
        Options_File = settingsFileAndPath; 
    }

/*----------------------------------------------------------
 * Function     : GetKey
 * Description  : gets the value of the key.
 * Parameters   : key
 * Return       : value of the key if key exist, null if not exist
 * --------------------------------------------------------*/
    public string GetKey(string key)
    {
        
      //  string value_of_each_key;
        string key_of_each_line;
        string line;
        int index;
        string key_value;
        key_value = null;
        
        sr = new StreamReader(Options_File);
        while (null != (line = sr.ReadLine()))
        {

            
            index = line.IndexOf("=");
           
            
           //    value_of_each_key = line.Substring(index+1);



            if (index >= 1)
            {
                key_of_each_line = line.Substring(0, index);
                if (key_of_each_line == key)
                {
                    key_value = line.Substring(key.Length + 1);
                }

            }
            else
            {
            }
            

        }
        sr.Close();
        return key_value;
    }
       

/*----------------------------------------------------------
 * Function     : SetKey
 * Description  : sets a value to the specified key
 * Parameters   : key and a value
 * Return       : none
 * --------------------------------------------------------*/
    public void SetKey(string key , string value)
    {
        bool key_was_found_inside_the_loop;
        string value_of_each_key;
        string key_of_each_line ;
        string line;
        int index;
        key_was_found_inside_the_loop = false;

        temp_settings_file = "\\temp_settings_file.txt";
        temp_settings_dir = path_exe + @"\temp_settings";
        if (!Directory.Exists(temp_settings_dir))
        {
            Directory.CreateDirectory(temp_settings_dir);
        }
        
        sw = new StreamWriter(temp_settings_dir+temp_settings_file);
        sr = new StreamReader(Options_File);
        while (null != (line = sr.ReadLine()))
        {
            
            index = line.IndexOf("=");
            key_of_each_line = line.Substring(0, index);
            value_of_each_key = line.Substring( index + 1);
         //   key_value = line.Substring(0,value.Length);
            if (key_of_each_line == key)
            {
                sw.WriteLine(key + "=" + value);
                key_was_found_inside_the_loop = true;

            }
            else
            {
               sw.WriteLine(key_of_each_line+"="+value_of_each_key);
            }

        }

        if (!key_was_found_inside_the_loop)
        {
            sw.WriteLine(key + "=" + value);
        }
        sr.Close();
        sw.Close();
        File.Delete(Options_File);
        File.Move(temp_settings_dir + temp_settings_file, Options_File);
        return;
        
    }



    public List<float> GetListFloatKey(string keys)
    {
        List<float> result = new List<float>();
        string s = GetKey(keys);
        if (s != null)
        {
            string[] items = s.Split(new char[] { ',' });
            float f;
            foreach (string item in items)
            {
                if (float.TryParse(item, out f))
                    result.Add(f);
            }
            return result;
        }
        else
        {
            return result;
        }
    }


    public void SetListFloatKey(string key, List<float> Values)
    {
        StringBuilder sb = new StringBuilder();
        foreach (float value in Values)
        {
            sb.AppendFormat(",{0}", value);
        }
        if (Values.Count == 0)
        {
            SetKey(key, "");
        }
        else
        {
            SetKey(key, sb.ToString().Substring(1));
        }
    }

    public List<int> GetListIntKey(string keys)
    {
        /*List<int> t = new List<int>();
        t = (GetListFloatKey(keys).ConvertAll(x => (int)x));
        return t;*/

        List<int> result = new List<int>();
        string s = GetKey(keys);
        if (s != null)
        {
            string[] items = s.Split(new char[] { ',' });
            int f;
            foreach (string item in items)
            {
                if (int.TryParse(item, out f))
                    result.Add(f);
            }
            return result;
        }
        else
        {
            return result;
        }
;

    }

    public void SetListIntKey(string key, List<int> Values)
    {

        StringBuilder sb = new StringBuilder();
        foreach (int value in Values)
        {
            sb.AppendFormat(",{0}", value);
        }
        if (Values.Count == 0)
        {
            SetKey(key, "");
        }
        else
        {
            SetKey(key, sb.ToString().Substring(1));
        }
    }
        /*---------------------------------
        *   P R I V A T E    M E T H O D S 
        * -------------------------------*/

    }
}
