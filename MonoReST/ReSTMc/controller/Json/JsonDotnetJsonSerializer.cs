﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emc.Documentum.Rest.Net
{
    /// <summary>
    /// JSON serializer in the Json.NET library
    /// </summary>
    public class JsonDotnetJsonSerializer : AbstractJsonSerializer
    {
        private JsonSerializer SERIALIZER;
        private Formatting _formatting = Formatting.Indented;
        /// <summary>
        /// Set/Get the formatting for the serializer. Options are:
        /// Formatting.None, Formatting.Indented
        /// </summary>
        public Formatting Formatting 
        {
            get { return _formatting; } 
            set { _formatting = value; } 
        }

        public bool PrintStreamBeforeDeserialize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public JsonDotnetJsonSerializer()
        {
            SERIALIZER = new JsonSerializer();
            SERIALIZER.Converters.Add(new JavaScriptDateTimeConverter());
            SERIALIZER.NullValueHandling = NullValueHandling.Ignore;
            SERIALIZER.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            SERIALIZER.MissingMemberHandling = MissingMemberHandling.Ignore;
            //SERIALIZER.TraceWriter = new MemoryTraceWriter();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public override T ReadObject<T>(Stream input)
        {
            if (PrintStreamBeforeDeserialize)
            {
                StreamReader sr = new StreamReader(input);
                string text = sr.ReadToEnd();
                Console.WriteLine("JSON Text of input object: \n--------------------------------------------------\n"
                    + text + "\n\n--------------------------------------------------");
                input.Position = 0;
            }
            JsonReader reader = new JsonTextReader(new StreamReader(input));
            T obj = SERIALIZER.Deserialize<T>(reader);
            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="output"></param>
        /// <param name="obj"></param>
        public override void WriteObject<T>(Stream output, T obj)
        {          
            JsonWriter writer = new JsonTextWriter(new StreamWriter(output));
            SERIALIZER.Serialize(writer, obj);
            writer.Flush();
            output.Position = 0;
            //Console.WriteLine(SERIALIZER.TraceWriter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public String Serialize<T>(T obj)
        {
            String json = "";
            using (MemoryStream ms = new MemoryStream()) 
            {
                JsonWriter writer = new JsonTextWriter(new StreamWriter(ms));
                writer.Formatting = Formatting;
                SERIALIZER.Serialize(writer, obj);
                json = Encoding.UTF8.GetString(ms.ToArray());
                return json;
            }
            
        }
    }
}
