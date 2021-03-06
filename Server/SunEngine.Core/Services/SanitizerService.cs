using System.Collections.Generic;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Configuration;
using SunEngine.Core.Configuration.Options;
using SunEngine.Core.Errors.Exceptions;

namespace SunEngine.Core.Services
{
	public class SanitizerService
	{
		private const string DefaultCategory = "Default";

		private readonly IConfiguration configuration;

		private Dictionary<string, Sanitizer> optionCategories = new Dictionary<string, Sanitizer>();

		public SanitizerOptions SanitizerOptions { get; protected set; }

		public SanitizerService(IConfiguration configuration)
		{
			this.configuration = configuration;
			Initialize();
		}

		public Sanitizer GetSanitizer(string sanitizerName = DefaultCategory)
		{
			return FindSanitizer(sanitizerName);
		}

		public string Sanitize(IHtmlDocument doc, string sanitizerName = DefaultCategory)
		{
			var sanitizer = FindSanitizer(sanitizerName);
			return sanitizer.Sanitize(doc);
		}

		public string Sanitize(string text, string sanitizerName = DefaultCategory)
		{
			var sanitizer = FindSanitizer(sanitizerName);
			return sanitizer.Sanitize(text);
		}

		private Sanitizer FindSanitizer(string sanitizerName)
		{
			if (optionCategories.ContainsKey(sanitizerName))
				return optionCategories[sanitizerName];

			throw new SunException($"Not found sanitizer with name \"{sanitizerName}\"");
		}

		public void Initialize()
		{
			optionCategories.Clear();

			var section = configuration.GetSection("Sanitizer");

			var key = DefaultCategory;
			SanitizerOptions = section.Get<SanitizerOptions>();
			var sanitizer = new Sanitizer(SanitizerOptions);
			optionCategories.Add(key, sanitizer);
		}
	}
}