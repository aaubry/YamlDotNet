using System;

namespace YamlDotNet.CoreCs
{
	public class SimpleKey
	{
		public bool isPossible;
		public bool isRequired;
		public int tokenNumber;
		public Mark mark;
		
		public bool IsPossible {
			get {
				return isPossible;
			}
			set {
				isPossible = value;
			}
		}

		public bool IsRequired {
			get {
				return isRequired;
			}
			set {
				isRequired = value;
			}
		}

		public int TokenNumber {
			get {
				return tokenNumber;
			}
			set {
				tokenNumber = value;
			}
		}

		public Mark Mark {
			get {
				return mark;
			}
			set {
				mark = value;
			}
		}
		
		public SimpleKey() {
		}
		
		public SimpleKey(bool isPossible, bool isRequired, int tokenNumber, Mark mark) {
			this.isPossible = isPossible;
			this.isRequired = isRequired;
			this.tokenNumber = tokenNumber;
			this.mark = mark;
		}
	}
}