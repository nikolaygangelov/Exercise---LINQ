namespace MusicHub
{
    using System;
    using System.Globalization;
    using System.Text;
    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main()
        {
            MusicHubDbContext context = new MusicHubDbContext();

            DbInitializer.ResetDatabase(context);

            
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
	    //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();


            //turning needed info about albums into a collection using nested anonymous objects
            //using less data
            var producedAlbums = context.Albums
                .Select(a => new
                {
                    a.ProducerId,
                    AlbumName = a.Name,
                    ReleaseDate = a.ReleaseDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture), //ignoring local system settings
                    ProducerName = a.Producer.Name,
                    TotalPrice = a.Price,
                    AlbumSongs = a.Songs
                        .Select(s => new
                        {
                            SongName = s.Name,
                            Price = s.Price,
                            SongWriterName = s.Writer.Name
                        })
                        .OrderByDescending(s => s.SongName)
                        .ThenBy(s => s.SongWriterName)
                        .ToList()
                })
                .AsNoTracking()  //detaching from change tracker
                .ToList()
                .Where(a => a.ProducerId == producerId)
                .OrderByDescending(a => a.TotalPrice);
                

            foreach (var album in producedAlbums)
            {
                //using AppendLine so that every printing to be on separate line
                sb.AppendLine($"-AlbumName: {album.AlbumName}");
                sb.AppendLine($"-ReleaseDate: {album.ReleaseDate}");
                sb.AppendLine($"-ProducerName: {album.ProducerName}");
                sb.AppendLine("-Songs:");

                int counter = 1;
                foreach (var song in album.AlbumSongs)
                {
                    sb.AppendLine($"---#{counter++}"); //counting songs
                    sb.AppendLine($"---SongName: {song.SongName}");
                    sb.AppendLine($"---Price: {song.Price:f2}");
                    sb.AppendLine($"---Writer: {song.SongWriterName}");
                }

                sb.AppendLine($"-AlbumPrice: {album.TotalPrice:f2}");
            }

            
            //using Trim() to get rid of white spaces
            return sb.ToString().Trim();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //turning needed info about songs into a collection using anonymous object
            //using less data
            var songsToExport = context.Songs
                .Select(s => new
                {
                    SongName = s.Name,
                    PerformersNames = s.SongPerformers
                                       .Select(sp => $"{sp.Performer.FirstName} {sp.Performer.LastName}"),
                    WriterName = s.Writer.Name,
                    AlbumProducer = s.Album.Producer.Name,
                    Duration = s.Duration
                })
                .AsNoTracking()  //detaching from change tracker
                .ToList()
                .Where(s => s.Duration.TotalSeconds > duration)
                .OrderBy(s => s.SongName)
                .ThenBy(s => s.WriterName);

            int counter = 1;
            foreach (var song in songsToExport)
            {
                //using AppendLine so that every printing to be on separate line
                sb
                  .AppendLine($"-Song #{counter++}") //counting songs
                  .AppendLine($"---SongName: {song.SongName}")
                  .AppendLine($"---Writer: {song.WriterName}");

                if (song.PerformersNames.Any())
                {
                    foreach (var performer in song.PerformersNames.OrderBy(p => p))
                    {
                        sb.AppendLine($"---Performer: {performer}");
                    }
                }

                sb
                  .AppendLine($"---AlbumProducer: {song.AlbumProducer}")
                  .AppendLine($"---Duration: {song.Duration}");
            }

            //using Trim() to get rid of white spaces
            return sb.ToString().Trim();
        }
    }
}
