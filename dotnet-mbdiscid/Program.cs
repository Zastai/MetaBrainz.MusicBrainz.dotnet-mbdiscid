﻿using System;
using System.Diagnostics;
using System.Text;

namespace MetaBrainz.MusicBrainz.DiscId;

internal static class Program {

  private static readonly TimeSpan TwoSeconds = new(0, 0, 2);

  [STAThread]
  private static int Main(string[] args) {
    try {
      {
        var sb = new StringBuilder();
        var defaultDevice = TableOfContents.DefaultDevice;
        foreach (var dev in TableOfContents.AvailableDevices) {
          if (sb.Length > 0) {
            sb.Append(", ");
          }
          sb.Append(dev);
          if (dev == defaultDevice) {
            sb.Append(" (default)");
          }
        }
        Console.WriteLine($"Available Devices   : {sb}");
      }
      var features = TableOfContents.AvailableFeatures;
      Console.WriteLine($"Supported Features  : {features}");
      Console.WriteLine();
      string? device = null;
      var launchSubmission = false;
      foreach (var arg in args) {
        switch (arg) {
          case "help":
          case "-?":
          case "/?": {
            Console.WriteLine("Usage: DiscId [OPTIONS] [DEVICE]");
            Console.WriteLine();
            Console.WriteLine("Available Options:");
            Console.WriteLine("  -noisrc     Disable reading of track ISRC values.");
            Console.WriteLine("  -nomcn      Disable reading of the media catalog number.");
            Console.WriteLine("  -notext     Disable reading of CD-TEXT info.");
            Console.WriteLine("  -launch     Launch the submission URL after showing the information.");
            Console.WriteLine("  -help, -?   Show this list of options.");
            return 0;
          }
          case "-launch":
          case "/launch":
            launchSubmission = true;
            break;
          case "-noisrc":
          case "/noisrc":
            features &= ~DiscReadFeature.TrackIsrc;
            break;
          case "-nomcn":
          case "/nomcn":
            features &= ~DiscReadFeature.MediaCatalogNumber;
            break;
          case "-notext":
          case "/notext":
            features &= ~DiscReadFeature.CdText;
            break;
          default:
            if (device != null) {
              throw new ArgumentException("Too many command line arguments given.");
            }
            device = arg;
            break;
        }
      }
      var toc = TableOfContents.ReadDisc(device, features);
      Console.WriteLine($"CD Device Used      : {toc.DeviceName}");
      Console.WriteLine($"Features Requested  : {features}");
      Console.WriteLine();
      if ((features & DiscReadFeature.MediaCatalogNumber) != 0) {
        Console.WriteLine($"Media Catalog Number: {toc.MediaCatalogNumber ?? "* not set *"}");
      }
      Console.WriteLine($"MusicBrainz Disc ID : {toc.DiscId}");
      Console.WriteLine($"FreeDB Disc ID      : {toc.FreeDbId}");
      Console.WriteLine($"Submission URL      : {toc.SubmissionUrl}");
      Console.WriteLine();
      var languages = toc.TextLanguages;
      if (languages?.Count > 0) {
        var text = toc.TextInfo;
        if (text?.Count > 0) {
          Console.WriteLine("CD-TEXT Information:");
          var idx = 0;
          foreach (var l in languages) {
            Console.WriteLine($"- Language: {l}");
            var ti = text[idx++];
            if (ti.Genre.HasValue) {
              if (ti.GenreDescription != null) {
                Console.WriteLine($"  - Genre           : {ti.Genre.Value} ({ti.GenreDescription})");
              }
              else {
                Console.WriteLine($"  - Genre           : {ti.Genre.Value}");
              }
            }
            if (ti.Identification != null) {
              Console.WriteLine($"  - Identification  : {ti.Identification}");
            }
            if (ti.ProductCode != null) {
              Console.WriteLine($"  - UPC/EAN         : {ti.ProductCode}");
            }
            if (ti.Title != null) {
              Console.WriteLine($"  - Title           : {ti.Title}");
            }
            if (ti.Performer != null) {
              Console.WriteLine($"  - Performer       : {ti.Performer}");
            }
            if (ti.Lyricist != null) {
              Console.WriteLine($"  - Lyricist        : {ti.Lyricist}");
            }
            if (ti.Composer != null) {
              Console.WriteLine($"  - Composer        : {ti.Composer}");
            }
            if (ti.Arranger != null) {
              Console.WriteLine($"  - Arranger        : {ti.Arranger}");
            }
            if (ti.Message != null) {
              Console.WriteLine($"  - Message         : {ti.Message}");
            }
          }
          Console.WriteLine();
        }
      }
      Console.WriteLine("Tracks:");
      {
        // Check for a "hidden" pre-gap track
        var t = toc.Tracks[toc.FirstTrack];
        if (t.StartTime > Program.TwoSeconds) {
          Console.WriteLine($" --- Offset: {150,6} ({Program.TwoSeconds,-16}) Length: {t.Offset - 150,6} " +
                            $"({t.StartTime.Subtract(Program.TwoSeconds),-16})");
        }
      }
      foreach (var t in toc.Tracks) {
        Console.Write($" {t.Number,2}. Offset: {t.Offset,6} ({t.StartTime,-16}) Length: {t.Length,6} ({t.Duration,-16})");
        if ((features & DiscReadFeature.TrackIsrc) != 0) {
          Console.Write($" ISRC: {t.Isrc ?? "* not set *"}");
        }
        Console.WriteLine();
        if (languages?.Count > 0) {
          var text = t.TextInfo;
          if (text?.Count > 0) {
            Console.WriteLine("     CD-TEXT Information:");
            var idx = 0;
            foreach (var l in languages) {
              Console.WriteLine($"     - Language: {l}");
              var ti = text[idx++];
              if (ti.Title != null) {
                Console.WriteLine($"       - Title     : {ti.Title}");
              }
              if (ti.Performer != null) {
                Console.WriteLine($"       - Performer : {ti.Performer}");
              }
              if (ti.Lyricist != null) {
                Console.WriteLine($"       - Lyricist  : {ti.Lyricist}");
              }
              if (ti.Composer != null) {
                Console.WriteLine($"       - Composer  : {ti.Composer}");
              }
              if (ti.Arranger != null) {
                Console.WriteLine($"       - Arranger  : {ti.Arranger}");
              }
              if (ti.Message != null) {
                Console.WriteLine($"       - Message   : {ti.Message}");
              }
              if (ti.Isrc != null) {
                Console.WriteLine($"       - ISRC      : {ti.Isrc}");
              }
            }
          }
        }
      }
      if (launchSubmission) {
        Process.Start(new ProcessStartInfo {
                        FileName = toc.SubmissionUrl.ToString(),
                        UseShellExecute = true
                      });
      }
    }
    catch (Exception e) {
      Program.ReportExceptionOnConsole(e);
      return 1;
    }
    return 0;
  }

  private static void ReportExceptionOnConsole(Exception? e, string prefix = "") {
    if (e == null) {
      return;
    }
    var currentColor = Console.ForegroundColor;
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
      Console.ForegroundColor = currentColor;
    }
  }

}
