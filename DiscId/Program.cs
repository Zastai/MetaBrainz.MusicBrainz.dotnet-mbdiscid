using System;
using System.Diagnostics;

namespace MetaBrainz.MusicBrainz.DiscId {

  internal static class Program {

    private static readonly TimeSpan TwoSeconds = new TimeSpan(0, 0, 2);

    private static void ReportExceptionOnConsole(Exception e, string prefix = "") {
      if (e == null)
        return;
      var curcolor = Console.ForegroundColor;
      try {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(prefix);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{e.GetType()}] {e.Message}");
#if DEBUG
        {
          var st = e.StackTrace;
          if (!string.IsNullOrEmpty(st)) {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(st);
          }
        }
#endif
        prefix = "Caused by: ";
        Program.ReportExceptionOnConsole(e.InnerException, prefix);
      }
      finally {
        Console.ForegroundColor = curcolor;
      }
    }

    private static int Main(string[] args) {
      try {
        var showText = true;
        string device = null;
        foreach (var arg in args) {
          switch (arg) {
            case "help": case "-?": case "/?":
              Console.WriteLine($"Supported Read Features: {string.Join(", ", TableOfContents.ReadFeatures)}");
              Console.WriteLine();
              Console.WriteLine($"Default Device: {TableOfContents.DefaultDevice ?? "<none available>"}");
              Console.WriteLine();
              Console.WriteLine("Available Devices:");
              var n = 0;
              foreach (var dev in TableOfContents.AvailableDevices)
                Console.WriteLine($"{++n,3}. {dev}");
              return 0;
            case "-notext":
            case "/notext":
              showText = false;
              break;
            default:
              if (device != null)
                throw new ArgumentException("Too many command line arguments given.");
              device = arg;
              break;
          }
        }
        var toc = TableOfContents.ReadDisc(device);
        if (toc == null)
          Console.WriteLine("No table of contents available.");
        else {
          Console.WriteLine($"CD Device Used      : {toc.DeviceName}");
          Console.WriteLine();
          Console.WriteLine($"Media Catalog Number: {toc.MediaCatalogNumber}");
          Console.WriteLine($"MusicBrainz Disc ID : {toc.DiscId}");
          Console.WriteLine($"FreeDB Disc ID      : {toc.FreeDbId}");
          Console.WriteLine($"Submission URL      : {toc.SubmissionUrl}");
          Console.WriteLine();
          var languages = toc.TextLanguages;
          if (showText && languages?.Count > 0) {
            var text = toc.TextInfo;
            if (text?.Count > 0) {
              Console.WriteLine("CD-TEXT Information:");
              var idx = 0;
              foreach (var l in languages) {
                Console.WriteLine($"- Language: {l}");
                var ti = text[idx++];
                if (ti.Genre.HasValue) {
                  if (ti.GenreDescription != null)
                    Console.WriteLine($"  - Genre           : {ti.Genre.Value} ({ti.GenreDescription})");
                  else
                    Console.WriteLine($"  - Genre           : {ti.Genre.Value}");
                }
                if (ti.Identification != null) Console.WriteLine($"  - Identification  : {ti.Identification}");
                if (ti.ProductCode != null) Console.WriteLine($"  - UPC/EAN         : {ti.ProductCode}");
                if (ti.Title != null) Console.WriteLine($"  - Title           : {ti.Title}");
                if (ti.Performer != null) Console.WriteLine($"  - Performer       : {ti.Performer}");
                if (ti.Lyricist != null) Console.WriteLine($"  - Lyricist        : {ti.Lyricist}");
                if (ti.Composer != null) Console.WriteLine($"  - Composer        : {ti.Composer}");
                if (ti.Arranger != null) Console.WriteLine($"  - Arranger        : {ti.Arranger}");
                if (ti.Message != null) Console.WriteLine($"  - Message         : {ti.Message}");
              }
              Console.WriteLine();
            }
          }
          Console.WriteLine("Tracks:");
          { // Check for a "hidden" pre-gap track
            var t = toc.Tracks[toc.FirstTrack];
            if (t.StartTime > Program.TwoSeconds)
              Console.WriteLine($" --- Offset: {150,6} ({Program.TwoSeconds,-16}) Length: {t.Offset - 150,6} ({t.StartTime.Subtract(Program.TwoSeconds),-16})");
          }
          foreach (var t in toc.Tracks) {
            Console.WriteLine($" {t.Number,2}. Offset: {t.Offset,6} ({t.StartTime,-16}) Length: {t.Length,6} ({t.Duration,-16}) ISRC: {t.Isrc}");
            if (showText && languages?.Count > 0) {
              var text = t.TextInfo;
              if (text?.Count > 0) {
                Console.WriteLine("     CD-TEXT Information:");
                var idx = 0;
                foreach (var l in languages) {
                  Console.WriteLine($"     - Language: {l}");
                  var ti = text[idx++];
                  if (ti.Title          != null) Console.WriteLine($"       - Title     : {ti.Title}");
                  if (ti.Performer      != null) Console.WriteLine($"       - Performer : {ti.Performer}");
                  if (ti.Lyricist       != null) Console.WriteLine($"       - Lyricist  : {ti.Lyricist}");
                  if (ti.Composer       != null) Console.WriteLine($"       - Composer  : {ti.Composer}");
                  if (ti.Arranger       != null) Console.WriteLine($"       - Arranger  : {ti.Arranger}");
                  if (ti.Message        != null) Console.WriteLine($"       - Message   : {ti.Message}");
                }
              }
            }
          }
        }
      }
      catch (Exception e) {
        Program.ReportExceptionOnConsole(e);
        return 1;
      }
      return 0;
    }

  }

}
