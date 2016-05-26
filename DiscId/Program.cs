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
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{prefix}[{e.GetType()}] {e.Message}");
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
        if (args.Length == 1 && (args[0] == "help" || args[0] == "-?" || args[0] == "/?")) {
          Console.WriteLine($"Supported Read Features: {string.Join(", ", TableOfContents.ReadFeatures)}");
          Console.WriteLine();
          Console.WriteLine($"Default Device: {TableOfContents.DefaultDevice ?? "<none available>"}");
          Console.WriteLine();
          Console.WriteLine("Available Devices:");
          var n = 0;
          foreach (var device in TableOfContents.AvailableDevices)
            Console.WriteLine($"{++n,3}. {device}");
        }
        else {
          var toc = TableOfContents.ReadDisc((args.Length == 0) ? null : args[0]);
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
            Console.WriteLine("Tracks:");
            { // Check for a "hidden" pre-gap track
              var t = toc.Tracks[toc.FirstTrack];
              if (t.StartTime > Program.TwoSeconds)
                Console.WriteLine($" --- Offset: {150,6} ({Program.TwoSeconds,-16}) Length: {t.Offset - 150,6} ({t.StartTime.Subtract(Program.TwoSeconds),-16})");
            }
            foreach (var t in toc.Tracks)
              Console.WriteLine($" {t.Number,2}. Offset: {t.Offset,6} ({t.StartTime,-16}) Length: {t.Length,6} ({t.Duration,-16}) ISRC: {t.Isrc}");
          }
        }
        return 0;
      }
      catch (Exception e) {
        Program.ReportExceptionOnConsole(e);
        return 1;
      }
      finally {
        if (Debugger.IsAttached)
          Console.ReadKey();
      }
    }

  }

}
