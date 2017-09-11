//using System;
//using System.Text;
//using System.IO;
//using System.Xml;	
//using System.Xml.Serialization;
//using System.Collections;
//using System.Collections.Specialized;
//
//namespace ExpoExchange.Common.Util.ExceptionManagement
//{
//	public class ExceptionXMLPublisher : BasePublisher 
//	{
//		public ExceptionXMLPublisher()
//		{
//		}
//
//		public override void Publish(Exception exception, NameValueCollection AdditionalInfo, NameValueCollection ConfigSettings)
//		{
//
//			XmlDocument ExceptionInfo = ExceptionManager.SerializeToXml(exception,AdditionalInfo);
//
//			try 
//			{
//				string filename;
//				if (ConfigSettings != null)
//				{
//					filename = ConfigSettings["fileName"];
//				}
//				else
//				{
//					filename = @"C:\ErrorLog.xml";
//				}
//				using ( FileStream fs =  File.Open(filename, FileMode.Append, FileAccess.ReadWrite) )
//				{
//					using ( StreamWriter writer = new StreamWriter(fs) )
//					{
//						writer.Write(ExceptionInfo.OuterXml);
//					}
//				}
//			}
//			catch
//			{
//				if (base.NextPublisher != null)
//					base.NextPublisher.Publish( exception,  AdditionalInfo,  ConfigSettings);		
//			}
//
//		}
//	}
//}
