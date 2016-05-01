using System;
using System.Diagnostics;

namespace MetaBrainz.MusicBrainz.DiscId {

  internal static class Program {

    private static readonly TimeSpan TwoSeconds = new TimeSpan(0, 0, 2);

    private static void Main(string[] args) {
      try {
      var device = (args.Length == 0) ? new CdDevice() : new CdDevice(args[0]);
        Console.WriteLine($"Disc Device: {device.Name}");
        Console.WriteLine();
        device.ReadDisc();
        var toc = device.TableOfContents;
        if (toc == null)
          Console.WriteLine("No table of contents available.");
        else {
          Console.WriteLine($"Media Catalog Number: {toc.MediaCatalogNumber}");
          Console.WriteLine($"MusicBrainz Disc ID : {toc.DiscId}");
          Console.WriteLine($"FreeDB Disc ID      : {toc.FreeDbId}");
          Console.WriteLine($"Submission URL      : {toc.SubmissionUrl}");
          Console.WriteLine();
          Console.WriteLine("Tracks:");
          {
            var t = toc.Tracks[toc.FirstTrack];
            if (t.StartTime > Program.TwoSeconds)
              Console.WriteLine($" --- Duration: {t.StartTime.Subtract(Program.TwoSeconds)}");
          }
          foreach (var t in toc.Tracks)
            Console.WriteLine($" {t.Number,2}. Duration: {t.Duration,-16} ISRC: {t.Isrc}");
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
