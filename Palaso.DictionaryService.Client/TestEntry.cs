using System;
using System.Collections.Generic;
using System.Text;
using Palaso.Text;

namespace Palaso.DictionaryService.Client
{
	internal class TestEntry : IEntry
	{
		private MultiTextBase _lexemeForms;
		private MultiTextBase _primaryDefinition;
		private MultiTextBase _primaryExample;
		private List<MultiTextBase> _variants;
		/// <summary>
		/// clients must use a factory method
		/// </summary>
		internal TestEntry()
		{
			_lexemeForms = new MultiTextBase();
			_primaryDefinition = new MultiTextBase();
			_primaryExample = new MultiTextBase();
			_variants = new List<MultiTextBase>();
		}

		internal MultiTextBase LexemeForms
		{
			get { return _lexemeForms; }
			set { _lexemeForms = value; }
		}

		public void AddLexemeForm(string writingSystemId, string form)
		{
			LexemeForms.SetAlternative(writingSystemId, form);
		}
		public void AddPrimaryDefinition(string writingSystemId, string definition)
		{
			_primaryDefinition.SetAlternative(writingSystemId, definition);
		}
		public void AddPrimaryExampleSentence(string writingSystemId, string example)
		{
			_primaryExample.SetAlternative(writingSystemId, example);
		}

		public string GetLexemeFormWithExactWritingSystem(string writingSystemId)
		{
			return LexemeForms.GetExactAlternative(writingSystemId);
		}



		public string GetHtmlArticle(ArticleCompositionFlags flags)
		{
			StringBuilder b = new StringBuilder();
			//NO: allow the client to build up html, don't assume this is the whole thing: b.Append("<html><body>");
			b.AppendFormat("<strong>{0}</strong>", _lexemeForms.GetFirstAlternative());
			if ((flags & ArticleCompositionFlags.Definition) != 0)
			{
				b.AppendFormat(@"{0}", _primaryDefinition.GetFirstAlternative());
			}
			b.AppendFormat(@" <em>{0}</em>", _primaryExample.GetFirstAlternative());
			return b.ToString();
		}

		public void AddInflectionalVariant(string writingSystemId, string variant)
		{
			MultiTextBase variantMt = new MultiTextBase();
			variantMt.SetAlternative(writingSystemId, variant);
			_variants.Add(variantMt);
		}

		public bool IsMatch(string writingSystemId, string form, FindMethods method)
		{
			List<string> forms = new List<string>();
			foreach (MultiTextBase variant in _variants)
			{
				forms.Add(variant.GetExactAlternative(writingSystemId));
			}
			forms.Add(LexemeForms.GetExactAlternative(writingSystemId));

			foreach (string s in forms)
			{
				if ((method == FindMethods.Exact && s == form)
					|| // just a hack to get the idea
					(method == FindMethods.DefaultApproximate && form.Contains(s)))
				{
					return true;
				}
			}
			return false;
		}
	}
}
