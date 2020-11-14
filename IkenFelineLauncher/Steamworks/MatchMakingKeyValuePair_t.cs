using System.Runtime.InteropServices;

namespace Steamworks
{
	public struct MatchMakingKeyValuePair_t
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		private byte[] m_szKey_;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		private byte[] m_szValue_;

		public string m_szKey
		{
			get
			{
				return InteropHelp.ByteArrayToStringUTF8(m_szKey_);
			}
			set
			{
				InteropHelp.StringToByteArrayUTF8(value, m_szKey_, 256);
			}
		}

		public string m_szValue
		{
			get
			{
				return InteropHelp.ByteArrayToStringUTF8(m_szValue_);
			}
			set
			{
				InteropHelp.StringToByteArrayUTF8(value, m_szValue_, 256);
			}
		}

		private MatchMakingKeyValuePair_t(string strKey, string strValue)
		{
			m_szKey_ = null;
			m_szValue_ = null;
			m_szKey = strKey;
			m_szValue = strValue;
		}
	}
}
