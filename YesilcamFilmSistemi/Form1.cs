using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace YesilcamFilmSistemi
{
    public partial class Form1 : Form
    {
        private Timer timer;

        NpgsqlConnection baglanti = new NpgsqlConnection("Server=localhost;Port=5432;Database=YesilcamFilmSistemi;User Id=postgres;Password=1234;Include Error Detail=true;");
        private bool BaglantiKontrol()
        {
            if (baglanti.State == ConnectionState.Closed)
            {
                try
                {
                    baglanti.Open();
                    return true; // Bağlantı başarılı şekilde açıldı.
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bağlantı Hatası: " + ex.Message);
                    return false; // Bağlantı açılamadı.
                }
            }
            else
            {
                return true; // Bağlantı zaten açık.
            }
        }
        public Form1()
        {
            InitializeComponent();

            // Timer'ı başlat
            timer = new Timer();
            timer.Interval = 750; // 750 milisaniye
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Timer her tetiklendiğinde lblTime kontrolünün görünürlüğünü değiştir
            lblTime.Visible = !lblTime.Visible;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            FilmleriGoster();
            dgvFilmler.Columns["filmid"].Visible = false;
            dgvFilmler.Columns["filmafisi"].Visible = false;
            OyunculariGoster();
            dgvOyuncular.Columns["oyuncuid"].Visible = false;

            YonetmenleriGoster();
            dgvYonetmenler.Columns["yonetmenid"].Visible = false;

            FilmTurleriGoster();
            dgvFilmTurleri.Columns["turid"].Visible = false;
        }
        private void lblListelereGit_Click(object sender, EventArgs e)
        {
            int currentindex = tab.SelectedIndex;
            tab.SelectedIndex = currentindex + 1;
        }
        private void OyuncununFilmleriniListele(string oyuncuAdi)
        {
            if (BaglantiKontrol())
            {
                try
                {
                    // Oyuncunun oynadığı filmleri getiren sorgu
                    string sorgu = "SELECT Film.FilmID, Film.Adi AS FilmAdi, Film.YapimYili " +
                                   "FROM Oyuncu " +
                                   "INNER JOIN Film ON Oyuncu.FilmID = Film.FilmID " +
                                   "WHERE Oyuncu.SahneAdiSoyadi = @OyuncuAdi";

                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@OyuncuAdi", oyuncuAdi);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Verileri gruplayan DataView oluştur
                    DataView dv = new DataView(dt);
                    dv.Sort = "YapimYili";  // Eğer başka bir sıralama yapmak istiyorsanız burayı düzeltebilirsiniz

                    // DataGridView kontrolüne verileri bind etme
                    dgvOyuncununFilmleri.DataSource = dv.ToTable(true, "YapimYili", "FilmAdi");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
        }
        private void YonetmeninFilmleriniVeOdulleriniGoster(string yonetmenAdi)
        {
            if (BaglantiKontrol())
            {
                try
                {
                    // Yönetmenin çektiği filmleri ve aldığı ödülleri getiren sorgu
                    string sorgu = "SELECT Yonetmen.YonetmenAdi, Film.Adi AS FilmAdi, Yonetmen.AldigiOdulSayisi " +
                                   "FROM Yonetmen " +
                                   "INNER JOIN Film ON Yonetmen.FilmID = Film.FilmID " +
                                   "WHERE Yonetmen.YonetmenAdi = @YonetmenAdi";

                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(sorgu, baglanti);
                    da.SelectCommand.Parameters.AddWithValue("@YonetmenAdi", yonetmenAdi);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Verileri gruplayan DataView oluştur
                    DataView dv = new DataView(dt);


                    // DataGridView kontrolüne verileri bind etme
                    dgvYonetmenFilmveOdul.DataSource = dv.ToTable(true, "YonetmenAdi", "FilmAdi", "AldigiOdulSayisi");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
        }
        private DataTable GetTopRatedMoviesByGenre(string turAdi)
        {
            DataTable resultTable = new DataTable();

            // Veritabanı sorgusu
            string query = @"
        SELECT
            Film.Adi AS FilmAdi,
            Yonetmen.YonetmenAdi,
            Oyuncu.SahneAdiSoyadi,
            Film.YapimYili,
            Film.Rating,
            FilmTuru.TurAdi
        FROM
            Film
        INNER JOIN
            Yonetmen ON Film.FilmID = Yonetmen.FilmID
        INNER JOIN
            Oyuncu ON Film.FilmID = Oyuncu.FilmID
        INNER JOIN
            FilmTuru ON Film.TurID = FilmTuru.TurID
        WHERE
            FilmTuru.TurAdi = @turAdi
        ORDER BY
            Film.Rating DESC
        LIMIT 10";

            BaglantiKontrol();

            using (NpgsqlCommand command = new NpgsqlCommand(query, baglanti))
            {
                command.Parameters.AddWithValue("@turAdi", turAdi);

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command);
                adapter.Fill(resultTable);
            }

            return resultTable;
        }


        private void FilmleriGoster()
        {
            if (BaglantiKontrol())
            {
                try
                {
                    string sorgu = "SELECT Film.filmid, Film.adi, Film.rating, Film.butce, Film.yapimyili, Film.gisesayisi, Film.filmafisi, FilmTuru.turadi " +
                           "FROM Film " +
                           "INNER JOIN filmturu ON Film.turiD = filmturu.turiD order by rating desc";
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(sorgu, baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvFilmler.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
        }

        private void OyunculariGoster()
        {
            if (BaglantiKontrol())
            {
                try
                {
                    string sorgu = "SELECT oyuncuid, sahneadisoyadi, gercekadisoyadi, dogumyili, Cinsiyet, aldigiodulsayisi, Film.adi AS filmAdi " +
                                   "FROM oyuncu " +
                                   "INNER JOIN Film ON oyuncu.filmid = Film.filmid order by adi";
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(sorgu, baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvOyuncular.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
        }
        private void YonetmenleriGoster()
        {
            if (BaglantiKontrol())
            {
                try
                {
                    string sorgu = "SELECT yonetmenid, yonetmenadi, dogumyili, cinsiyet, aldigiodulsayisi, Film.adi AS filmadi " +
                                   "FROM yonetmen " +
                                   "INNER JOIN Film ON yonetmen.filmid = Film.filmid";
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(sorgu, baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvYonetmenler.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
        }
        private void FilmTurleriGoster()
        {
            if (BaglantiKontrol())
            {
                try
                {
                    string sorgu = "SELECT * FROM filmturu";
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(sorgu, baglanti);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvFilmTurleri.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
        }
        private void dgvFilmler_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            string headerText = dgvFilmler.Columns[e.ColumnIndex].HeaderText;

            // Sadece "Butce" ve "GiseSayisi" sütunları için kontrol yapalım
            if (headerText.Equals("butce") || headerText.Equals("gisesayisi"))
            {
                // Giriş değeri boş mu kontrolü
                if (string.IsNullOrEmpty(e.FormattedValue?.ToString()))
                {
                    MessageBox.Show($"{headerText} alanı boş bırakılamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = false; // İşlemi iptal et
                    return;
                }

                // Giriş değeri sayıya dönüştürülebilir mi kontrolü
                if (!int.TryParse(e.FormattedValue.ToString(), out _))
                {
                    MessageBox.Show($"{headerText} alanına sadece sayı girebilirsiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true; // İşlemi devam ettir ama uyarını ver
                }
            }
        }
        private void btnFilmListesiniGüncelle_Click(object sender, EventArgs e)
        {
            if (BaglantiKontrol())
            {
                try
                {
                    // DataGridView'deki değişiklikleri al
                    DataTable dt = (DataTable)dgvFilmler.DataSource;

                    // Güncellenen veya eklenen satırları seç
                    DataRow[] güncellenenSatırlar = dt.Select(null, null, DataViewRowState.ModifiedCurrent);
                    DataRow[] eklenenSatırlar = dt.Select(null, null, DataViewRowState.Added);

                    // Eklenen satırları veritabanına uygula
                    foreach (DataRow satır in eklenenSatırlar)
                    {
                        // Rating ve ödül sayısı değeri kontrolü
                        if (!RatingKontrolu(satır["rating"].ToString()) || !YapimYiliKontrolu(satır["yapimyili"].ToString()))
                            return;

                        // Diğer kontrolleri burada ekleyebilirsiniz

                        // FilmID değerini almak için maksimum FilmID'yi sorgula
                        NpgsqlCommand maxIdCmd = new NpgsqlCommand("SELECT MAX(FilmID) FROM Film", baglanti);
                        int maxId = Convert.ToInt32(maxIdCmd.ExecuteScalar());

                        NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO Film (FilmID, Adi, Rating, Butce, YapimYili, GiseSayisi, FilmAfisi, TurID) VALUES (@FilmID, @Adi, @Rating, @Butce, @YapimYili, @GiseSayisi, @FilmAfisi, (SELECT TurID FROM FilmTuru WHERE TurAdi = @TurAdi))", baglanti);
                        cmd.Parameters.AddWithValue("@FilmID", maxId + 1); // Maksimum FilmID'yi 1 artırarak yeni bir değer al
                        cmd.Parameters.AddWithValue("@Adi", satır["adi"]);
                        cmd.Parameters.AddWithValue("@Rating", satır["rating"]);
                        cmd.Parameters.AddWithValue("@Butce", satır["butce"]);
                        cmd.Parameters.AddWithValue("@YapimYili", satır["yapimyili"]);
                        cmd.Parameters.AddWithValue("@GiseSayisi", satır["gisesayisi"]);
                        cmd.Parameters.AddWithValue("@FilmAfisi", satır["filmafisi"]);
                        cmd.Parameters.AddWithValue("@TurAdi", satır["turadi"]);
                        cmd.ExecuteNonQuery();
                    }

                    // Güncellenen satırları veritabanına uygula
                    foreach (DataRow satır in güncellenenSatırlar)
                    {
                        // Rating ve yapimyili değeri kontrolü
                        if (!RatingKontrolu(satır["rating"].ToString()) || !YapimYiliKontrolu(satır["yapimyili"].ToString()))
                            return;

                        // Diğer kontrolleri burada ekleyebilirsiniz

                        NpgsqlCommand cmd = new NpgsqlCommand("UPDATE Film SET Adi = @Adi, Rating = @Rating, Butce = @Butce, YapimYili = @YapimYili, GiseSayisi = @GiseSayisi, FilmAfisi = @FilmAfisi, TurID = (SELECT TurID FROM FilmTuru WHERE TurAdi = @TurAdi) WHERE FilmID = @FilmID", baglanti);
                        cmd.Parameters.AddWithValue("@Adi", satır["adi"]);
                        cmd.Parameters.AddWithValue("@Rating", satır["rating"]);
                        cmd.Parameters.AddWithValue("@Butce", satır["butce"]);
                        cmd.Parameters.AddWithValue("@YapimYili", satır["yapimyili"]);
                        cmd.Parameters.AddWithValue("@GiseSayisi", satır["gisesayisi"]);
                        cmd.Parameters.AddWithValue("@FilmAfisi", satır["filmafisi"]);
                        cmd.Parameters.AddWithValue("@TurAdi", satır["turadi"]);
                        cmd.Parameters.AddWithValue("@FilmID", satır["filmid"]);
                        cmd.ExecuteNonQuery();
                    }

                    // Veritabanından güncellenmiş verileri al
                    dt.Clear();
                    string sorgu = "SELECT Film.filmid, Film.adi, Film.rating, Film.butce, Film.yapimyili, Film.gisesayisi, Film.filmafisi, FilmTuru.turadi " +
                                   "FROM Film " +
                                   "INNER JOIN filmturu ON Film.turiD = filmturu.turiD order by rating desc";
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(sorgu, baglanti);
                    da.Fill(dt);

                    MessageBox.Show("Değişiklikler başarıyla kaydedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
        }

        private void btnFilmListesindenFilmSil_Click(object sender, EventArgs e)
        {
            if (dgvFilmler.SelectedRows.Count > 0)
            {
                // Confirm deletion with a Yes/No dialog
                DialogResult result = MessageBox.Show("Seçili filmi silmek istediğinize emin misiniz?", "Film Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (BaglantiKontrol())
                    {
                        try
                        {
                            int filmID = Convert.ToInt32(dgvFilmler.SelectedRows[0].Cells["filmid"].Value);

                            // Önce oyuncuları sil
                            NpgsqlCommand deleteOyuncularCmd = new NpgsqlCommand("DELETE FROM Oyuncu WHERE FilmID = @FilmID", baglanti);
                            deleteOyuncularCmd.Parameters.AddWithValue("@FilmID", filmID);
                            deleteOyuncularCmd.ExecuteNonQuery();

                            // Film'i sil
                            NpgsqlCommand deleteFilmCmd = new NpgsqlCommand("DELETE FROM Film WHERE FilmID = @FilmID", baglanti);
                            deleteFilmCmd.Parameters.AddWithValue("@FilmID", filmID);
                            deleteFilmCmd.ExecuteNonQuery();

                            // Veritabanından güncellenmiş verileri al ve DataGridView'ı güncelle
                            DataTable dtFilmler = (DataTable)dgvFilmler.DataSource;
                            dtFilmler.Clear();
                            string sorguFilmler = "SELECT Film.filmid, Film.adi, Film.rating, Film.butce, Film.yapimyili, Film.gisesayisi, Film.filmafisi, FilmTuru.turadi " +
                                                   "FROM Film " +
                                                   "INNER JOIN filmturu ON Film.turiD = filmturu.turiD order by rating desc";
                            NpgsqlDataAdapter daFilmler = new NpgsqlDataAdapter(sorguFilmler, baglanti);
                            daFilmler.Fill(dtFilmler);

                            // Oyuncuları göster ve DataGridView'ı güncelle
                            OyunculariGoster();

                            MessageBox.Show("Film ve ilişkili oyuncular başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Hata: " + ex.Message);
                        }
                        finally
                        {
                            baglanti.Close();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek istediğiniz filmi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnOyuncuListesiniGuncelle_Click(object sender, EventArgs e)
        {
            // DataGridView'deki değişiklikleri al
            DataTable dt = (DataTable)dgvOyuncular.DataSource;

            // Güncellenen veya eklenen satırları seç
            DataRow[] güncellenenSatırlar = dt.Select(null, null, DataViewRowState.ModifiedCurrent);
            DataRow[] eklenenSatırlar = dt.Select(null, null, DataViewRowState.Added);

            if (BaglantiKontrol())
            {
                try
                {
                    // Eklenen satırları veritabanına uygula
                    foreach (DataRow satır in eklenenSatırlar)
                    {
                        // Dogum yılı ve ödül sayısı değeri kontrolü
                        if (!DogumYiliKontrolu(satır["dogumyili"].ToString()) || !AldigiOdulKontrolu(satır["aldigiodulsayisi"].ToString()))
                            return;

                        string filmAdi = satır["filmAdi"].ToString();

                        // Check if the film exists in the Film table
                        NpgsqlCommand filmCheckCmd = new NpgsqlCommand("SELECT COUNT(*) FROM Film WHERE Adi = @FilmAdi", baglanti);
                        filmCheckCmd.Parameters.AddWithValue("@FilmAdi", filmAdi);

                        int filmCount = Convert.ToInt32(filmCheckCmd.ExecuteScalar());

                        if (filmCount == 0)
                        {
                            MessageBox.Show("Film bulunamadı: " + filmAdi, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Cinsiyet değerini büyük harfe çevir
                        string cinsiyet = satır["cinsiyet"].ToString().ToUpper();

                        NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO Oyuncu (SahneAdiSoyadi, GercekAdiSoyadi, dogumyili, Cinsiyet, AldigiOdulSayisi, FilmID) VALUES (@SahneAdiSoyadi, @GercekAdiSoyadi, @DogumTarihi, @Cinsiyet, @AldigiOdulSayisi, " +
                            "(SELECT FilmID FROM Film WHERE Adi = @FilmAdi OR @FilmAdi = ''))", baglanti);
                        cmd.Parameters.AddWithValue("@SahneAdiSoyadi", satır["sahneadisoyadi"]);
                        cmd.Parameters.AddWithValue("@GercekAdiSoyadi", satır["gercekadisoyadi"]);
                        cmd.Parameters.AddWithValue("@DogumTarihi", satır["dogumyili"]);
                        cmd.Parameters.AddWithValue("@Cinsiyet", satır["cinsiyet"]);
                        cmd.Parameters.AddWithValue("@AldigiOdulSayisi", satır["aldigiodulsayisi"]);
                        cmd.Parameters.AddWithValue("@FilmAdi", filmAdi);
                        cmd.ExecuteNonQuery();
                    }

                    // Güncellenen satırları veritabanına uygula
                    foreach (DataRow satır in güncellenenSatırlar)
                    {
                        // Dogum yılı ve ödül sayısı değeri kontrolü
                        if (!DogumYiliKontrolu(satır["dogumyili"].ToString()) || !AldigiOdulKontrolu(satır["aldigiodulsayisi"].ToString()))
                            return;

                        // Cinsiyet değerini büyük harfe çevir
                        string cinsiyet = satır["cinsiyet"].ToString().ToUpper();

                        string filmAdi = satır["filmAdi"].ToString();
                        NpgsqlCommand cmd = new NpgsqlCommand("UPDATE Oyuncu SET SahneAdiSoyadi = @SahneAdiSoyadi, GercekAdiSoyadi = @GercekAdiSoyadi, dogumyili = @DogumTarihi, " +
                            "Cinsiyet = @Cinsiyet, AldigiOdulSayisi = @AldigiOdulSayisi, FilmID = (SELECT FilmID FROM Film WHERE Adi = @FilmAdi OR @FilmAdi = '') WHERE OyuncuID = @OyuncuID", baglanti);
                        cmd.Parameters.AddWithValue("@SahneAdiSoyadi", satır["sahneadisoyadi"]);
                        cmd.Parameters.AddWithValue("@GercekAdiSoyadi", satır["gercekadisoyadi"]);
                        cmd.Parameters.AddWithValue("@DogumTarihi", satır["dogumyili"]);
                        cmd.Parameters.AddWithValue("@Cinsiyet", satır["cinsiyet"]);
                        cmd.Parameters.AddWithValue("@AldigiOdulSayisi", satır["aldigiodulsayisi"]);
                        cmd.Parameters.AddWithValue("@FilmAdi", filmAdi);
                        cmd.Parameters.AddWithValue("@OyuncuID", satır["oyuncuid"]);
                        cmd.ExecuteNonQuery();
                    }

                    // Veritabanından güncellenmiş verileri al
                    dt.Clear();
                    OyunculariGoster();

                    MessageBox.Show("Değişiklikler başarıyla kaydedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
        }
        private void btnOyuncuSil_Click(object sender, EventArgs e)
        {
            // Check if any row is selected
            if (dgvOyuncular.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz oyuncuyu seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the selected row
            DataGridViewRow selectedRow = dgvOyuncular.SelectedRows[0];

            // Get the OyuncuID of the selected player
            int oyuncuID = Convert.ToInt32(selectedRow.Cells["oyuncuid"].Value);

            // Confirm deletion with a Yes/No dialog
            DialogResult result = MessageBox.Show("Seçili oyuncuyu silmek istediğinize emin misiniz?", "Oyuncu Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (BaglantiKontrol())
                {
                    try
                    {
                        // Delete the player from the database
                        NpgsqlCommand cmd = new NpgsqlCommand("DELETE FROM Oyuncu WHERE OyuncuID = @OyuncuID", baglanti);
                        cmd.Parameters.AddWithValue("@OyuncuID", oyuncuID);
                        cmd.ExecuteNonQuery();

                        // Refresh the DataGridView
                        DataTable dt = (DataTable)dgvOyuncular.DataSource;
                        dt.Clear();
                        OyunculariGoster();

                        MessageBox.Show("Oyuncu başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hata: " + ex.Message);
                    }
                    finally
                    {
                        baglanti.Close();
                    }
                }
            }
        }
        private void btnYonetmenListesiniGuncelle_Click(object sender, EventArgs e)
        {
            // DataGridView'deki değişiklikleri al
            DataTable dt = (DataTable)dgvYonetmenler.DataSource;

            // Güncellenen veya eklenen satırları seç
            DataRow[] güncellenenSatırlar = dt.Select(null, null, DataViewRowState.ModifiedCurrent);
            DataRow[] eklenenSatırlar = dt.Select(null, null, DataViewRowState.Added);

            if (BaglantiKontrol())
            {
                try
                {
                    // Eklenen satırları veritabanına uygula
                    foreach (DataRow satır in eklenenSatırlar)
                    {
                        // Dogum yılı değeri kontrolü
                        if (!DogumYiliKontrolu(satır["dogumyili"].ToString()))
                            return;

                        // Cinsiyet değerini büyük harfe çevir
                        string cinsiyet = satır["cinsiyet"].ToString().ToUpper();
                        string filmAdi = satır["filmadi"].ToString();

                        // Check if the film exists in the Film table
                        NpgsqlCommand filmCheckCmd = new NpgsqlCommand("SELECT COUNT(*) FROM Film WHERE Adi = @FilmAdi", baglanti);
                        filmCheckCmd.Parameters.AddWithValue("@FilmAdi", filmAdi);

                        int filmCount = Convert.ToInt32(filmCheckCmd.ExecuteScalar());

                        if (filmCount == 0)
                        {
                            MessageBox.Show("Film bulunamadı: " + filmAdi + "\nLütfen filmler tablosuna ekleyin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO Yonetmen (YonetmenAdi, DogumYili, Cinsiyet, AldigiOdulSayisi, FilmID) VALUES (@YonetmenAdi, @DogumYili, @Cinsiyet, @AldigiOdulSayisi, (SELECT FilmID FROM Film WHERE Adi = @FilmAdi))", baglanti);
                        cmd.Parameters.AddWithValue("@YonetmenAdi", satır["yonetmenadi"]);
                        cmd.Parameters.AddWithValue("@DogumYili", satır["dogumyili"]);
                        cmd.Parameters.AddWithValue("@Cinsiyet", satır["cinsiyet"]);
                        cmd.Parameters.AddWithValue("@AldigiOdulSayisi", satır["aldigiodulsayisi"]);
                        cmd.Parameters.AddWithValue("@FilmAdi", filmAdi);
                        cmd.ExecuteNonQuery();
                    }


                    // Güncellenen satırları veritabanına uygula
                    foreach (DataRow satır in güncellenenSatırlar)
                    {
                        // Dogum yılı değeri kontrolü
                        if (!DogumYiliKontrolu(satır["dogumyili"].ToString()))
                            return;

                        // Cinsiyet değerini büyük harfe çevir
                        string cinsiyet = satır["cinsiyet"].ToString().ToUpper();

                        NpgsqlCommand cmd = new NpgsqlCommand("UPDATE Yonetmen SET YonetmenAdi = @YonetmenAdi, DogumYili = @DogumYili, Cinsiyet = @Cinsiyet, AldigiOdulSayisi = @AldigiOdulSayisi, FilmID = (SELECT FilmID FROM Film WHERE Adi = @FilmAdi) WHERE YonetmenID = @YonetmenID", baglanti);
                        cmd.Parameters.AddWithValue("@YonetmenAdi", satır["yonetmenadi"]);
                        cmd.Parameters.AddWithValue("@DogumYili", satır["dogumyili"]);
                        cmd.Parameters.AddWithValue("@Cinsiyet", satır["cinsiyet"]);
                        cmd.Parameters.AddWithValue("@AldigiOdulSayisi", satır["aldigiodulsayisi"]);
                        cmd.Parameters.AddWithValue("@FilmAdi", satır["filmadi"]);
                        cmd.Parameters.AddWithValue("@YonetmenID", satır["yonetmenid"]);
                        cmd.ExecuteNonQuery();
                    }

                    // Veritabanından güncellenmiş verileri al
                    dt.Clear();
                    YonetmenleriGoster();

                    MessageBox.Show("Değişiklikler başarıyla kaydedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
        }
        private void btnYonetmenSil_Click(object sender, EventArgs e)
        {
            if (dgvYonetmenler.SelectedRows.Count > 0)
            {
                // Confirm deletion with a Yes/No dialog
                DialogResult result = MessageBox.Show("Seçili yönetmeni silmek istediğinize emin misiniz?", "Yönetmen Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (BaglantiKontrol())
                    {
                        try
                        {
                            int filmid = Convert.ToInt32(dgvYonetmenler.SelectedRows[0].Cells["yonetmenid"].Value);

                            // Yönetmeni sil
                            NpgsqlCommand deleteYonetmenCmd = new NpgsqlCommand("DELETE FROM Yonetmen WHERE yonetmenid = @yonetmenid", baglanti);
                            deleteYonetmenCmd.Parameters.AddWithValue("@yonetmenid", filmid);
                            deleteYonetmenCmd.ExecuteNonQuery();

                            // Veritabanından güncellenmiş verileri al ve DataGridView'ı güncelle
                            DataTable dtYonetmenler = (DataTable)dgvYonetmenler.DataSource;
                            dtYonetmenler.Clear();
                            YonetmenleriGoster();

                            MessageBox.Show("Yönetmen ve bağlı olduğu filmler başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Hata: " + ex.Message);
                        }
                        finally
                        {
                            baglanti.Close();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek istediğiniz yönetmeni seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnFilmTurleriListesiniGuncelle_Click(object sender, EventArgs e)
        {
            // DataGridView'deki değişiklikleri al
            DataTable dt = (DataTable)dgvFilmTurleri.DataSource;

            // Güncellenen veya eklenen satırları seç
            DataRow[] güncellenenSatırlar = dt.Select(null, null, DataViewRowState.ModifiedCurrent);
            DataRow[] eklenenSatırlar = dt.Select(null, null, DataViewRowState.Added);

            if (BaglantiKontrol())
            {
                try
                {
                    // Eklenen satırları veritabanına uygula
                    foreach (DataRow satır in eklenenSatırlar)
                    {
                        string turAdi = satır["turadi"].ToString().ToLower();

                        NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO filmturu (turadi) VALUES (@TurAdi)", baglanti);
                        cmd.Parameters.AddWithValue("@TurAdi", satır["turadi"]);
                        cmd.ExecuteNonQuery();
                    }

                    // Güncellenen satırları veritabanına uygula
                    foreach (DataRow satır in güncellenenSatırlar)
                    {
                        string turAdi = satır["turadi"].ToString().ToLower();

                        NpgsqlCommand cmd = new NpgsqlCommand("UPDATE filmturu SET turadi = @TurAdi WHERE turid = @TurID", baglanti);
                        cmd.Parameters.AddWithValue("@TurAdi", satır["turadi"]);
                        cmd.Parameters.AddWithValue("@TurID", satır["turid"]);
                        cmd.ExecuteNonQuery();
                    }


                    // Veritabanından güncellenmiş verileri al
                    dt.Clear();
                    FilmTurleriGoster();

                    MessageBox.Show("Değişiklikler başarıyla kaydedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
        }
        private void btnTurSil_Click(object sender, EventArgs e)
        {
            if (dgvFilmTurleri.SelectedRows.Count > 0)
            {
                // Confirm deletion with a Yes/No dialog
                DialogResult result = MessageBox.Show("Seçili türü ve bağlı olduğu filmleri silmek istediğinize emin misiniz?", "Tür Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (BaglantiKontrol())
                    {
                        try
                        {
                            int turID = Convert.ToInt32(dgvFilmTurleri.SelectedRows[0].Cells["turiD"].Value);

                            // İlgili türün bağlı olduğu filmleri sil
                            NpgsqlCommand deleteFilmsCmd = new NpgsqlCommand("DELETE FROM Film WHERE turiD = @TurID", baglanti);
                            deleteFilmsCmd.Parameters.AddWithValue("@TurID", turID);
                            deleteFilmsCmd.ExecuteNonQuery();

                            // Türü sil
                            NpgsqlCommand deleteTurCmd = new NpgsqlCommand("DELETE FROM FilmTuru WHERE turiD = @TurID", baglanti);
                            deleteTurCmd.Parameters.AddWithValue("@TurID", turID);
                            deleteTurCmd.ExecuteNonQuery();

                            // Veritabanından güncellenmiş verileri al ve DataGridView'ı güncelle
                            DataTable dtTurler = (DataTable)dgvFilmTurleri.DataSource;
                            dtTurler.Clear();
                            FilmTurleriGoster();

                            MessageBox.Show("Tür ve bağlı olduğu filmler başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Hata: " + ex.Message);
                        }
                        finally
                        {
                            baglanti.Close();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek istediğiniz türü seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        // KONTROLLER 
        private bool RatingKontrolu(string rating)
        {
            // Rating değeri sayıya dönüştürülebiliyor mu kontrolü
            if (!double.TryParse(rating, out double parsedRating))
            {
                MessageBox.Show("Rating değeri sayı olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Rating değeri 1 ile 10 arasında mı kontrolü
            if (parsedRating < 1 || parsedRating > 10)
            {
                MessageBox.Show("Rating değeri 1 ile 10 arasında olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
        private bool DogumYiliKontrolu(string birthYear)
        {
            // Dogum yılı değeri sayıya dönüştürülebiliyor mu kontrolü
            if (!int.TryParse(birthYear, out int parsedYear))
            {
                MessageBox.Show("Doğum yılı sayı olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Doğum yılı değeri kontrolü (örneğin, belirli bir sınır aralığında olmalıdır)
            if (parsedYear < 1900 || parsedYear > DateTime.Now.Year)
            {
                MessageBox.Show("Geçerli bir doğum yılı giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
        private bool AldigiOdulKontrolu(string value)
        {
            int awardCount;
            return int.TryParse(value, out awardCount) && awardCount >= 0; // Sayıya dönüşebilir ve 0'dan büyük veya eşitse true döner
        }
        private bool YapimYiliKontrolu(string year)
        {
            if (!int.TryParse(year, out int parsedYear))
            {
                MessageBox.Show("Yapım yılı sayı olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Örneğin, bir sınırlama yapabilirsiniz:
            if (parsedYear < 1800 || parsedYear > DateTime.Now.Year)
            {
                MessageBox.Show("Geçerli bir yapım yılı giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void tab_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Hangi sekmenin seçildiğini kontrol edin
            if (tab.SelectedTab == tabPageListeler)
            {
                // Page2'nin scroll konumunu en üste getir
                tabPageListeler.VerticalScroll.Value = 0;
            }
        }
        private void btnOyuncununFilmleriGetir_Click(object sender, EventArgs e)
        {
            OyuncununFilmleriniListele(tbxOyuncununAdi.Text);
        }

        private void btnYonetmenFilmleriniOdulleriniGetir_Click(object sender, EventArgs e)
        {
            YonetmeninFilmleriniVeOdulleriniGoster(textBox1.Text);
        }

        private void btnTop10FilmGetir_Click(object sender, EventArgs e)
        {
            // Fonksiyonu çağır ve sonuçları DataGridView'e yükle
            DataTable resultTable = GetTopRatedMoviesByGenre(tbxTurAdi.Text);
            dgvTop10Film.DataSource = resultTable;
        }
        // BONUS FİLM AFİŞİİ
        private void FilmafisiGoster(string filmID)
        {
            if (BaglantiKontrol())
            {
                try
                {
                    string sorgu = "SELECT filmafisi FROM Film WHERE filmid = @FilmID";
                    using (NpgsqlCommand command = new NpgsqlCommand(sorgu, baglanti))
                    {
                        command.Parameters.AddWithValue("@FilmID", filmID);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Veritabanından alınan byte dizisini Image'a dönüştür
                                byte[] imageData = (byte[])reader["filmafisi"];
                                MemoryStream ms = new MemoryStream(imageData);
                                Image img = Image.FromStream(ms);

                                // PictureBox kontrolüne resmi ata
                                pictureBox1.Image = img;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    baglanti.Close();
                }
            }
        }

        private void dgvOyuncular_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvFilmler.Rows.Count)
            {
                string filmID = dgvFilmler.Rows[e.RowIndex].Cells["filmid"].Value.ToString();
                FilmafisiGoster(filmID);
            }
        }
        private void dgvFilmler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Herhangi bir hücreye tıklanıldığında resmi göster
            if (e.RowIndex >= 0 && e.RowIndex < dgvFilmler.Rows.Count)
            {
                DataGridViewRow selectedRow = dgvFilmler.Rows[e.RowIndex];

                // Film ID değerini al
                object filmIDValue = selectedRow.Cells["FilmID"].Value;

                // Film ID değeri null veya boş değilse işlemleri yap
                if (filmIDValue != null && filmIDValue != DBNull.Value)
                {
                    int filmID = Convert.ToInt32(filmIDValue);

                    // Film ID'ye göre resim dosyasının yolunu oluştur
                    string resimDosyaYolu = Path.Combine(@"C:\Users\ahmet\Desktop\YesilcamFilmSistemi\Afisler\", filmID + ".jpg");

                    // Dosyanın varlığını kontrol et
                    if (File.Exists(resimDosyaYolu))
                    {
                        try
                        {
                            // PictureBox kontrolüne resmi yükle
                            pictureBox1.Image = Image.FromFile(resimDosyaYolu);
                            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Resim yüklenirken bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Belirtilen resim dosyası bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // Film ID değeri null veya boşsa bir işlem yapma
                    // veya bir uyarı mesajı gösterilebilir.
                }
            }
        }
    }
}
