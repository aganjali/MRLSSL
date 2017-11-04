using System;

namespace FormulaEvaluator
{
	/// <summary>
	/// Summary description for ErrorException.
	/// </summary>
	public class ErrorException:Exception 
	{
		public ErrorException(long MessageId,string Message)
		{
			_message =Message;
			_messageId = MessageId;
		}
		public ErrorException(string Message)
		{
			_message =Message;
		}
		public ErrorException(long MessageId)
		{
			_messageId = MessageId;
		}
		public string _message; 
		public long  _messageId;
	}
}
