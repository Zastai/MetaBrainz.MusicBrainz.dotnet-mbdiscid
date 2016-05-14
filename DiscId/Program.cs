using System;
using System.Diagnostics;

namespace MetaBrainz.MusicBrainz.DiscId {

  internal static class Program {

    private static readonly TimeSpan TwoSeconds = new TimeSpan(0, 0, 2);

    private static void Main(string[] args) {
      try {
        if (args.Length == 1 && (args[0] == "help" || args[0] == "-?" || args[0] == "/?")) {
          Console.WriteLine($"Supported Features: {string.Join(", ", CdDevice.Features)}");
          Console.WriteLine();
          Console.WriteLine("Available Devices:");
          for (byte n = 0; n < 100; ++n) {
            var device = CdDevice.GetName(n);
            if (device == null)
              break;
            Console.WriteLine($"{n + 1,3}. {device}");
          }
        }
        else {
          var cd = new CdDevice();
          cd.ReadDisc((args.Length == 0) ? null : args[0]);
          var toc = cd.TableOfContents;
          if (toc == null)
            Console.WriteLine("No table of contents available.");
          else {
            Console.WriteLine($"CD Device Used      : {cd.DeviceName}");
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
      }
      catch (Exception e) {
        Console.WriteLine($"[error] {e}");
      }
      if (Debugger.IsAttached)
        Console.ReadKey();
    }

  }

}
