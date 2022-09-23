using System;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data;
using System.Drawing;
using OtelOtomasyon.Properties;

namespace OtelOtomasyon
{

    // Kayıt Ekle
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        OleDbConnection baglanti;
        OleDbCommand komut;
        DataSet ds;
        string dosyaYolu = Settings1.Default.DosyaYolu;

        private void Form1_Load(object sender, EventArgs e)
        {
            if (dosyaYolu == null)
            {
                MessageBox.Show("Uygulama başlatılmadan önce mutlaka kullanıcıların kaydedileceği bir Access veritabanı dosyası seçilmesi gereklidir.", "Veri tabanı dosyası bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (dosyaSec.ShowDialog() == DialogResult.OK)
                {
                    dosyaYolu = dosyaSec.FileName;
                    Settings1.Default.DosyaYolu = dosyaYolu;
                }
                else Application.Exit();
            }

            if(dosyaYolu != null)
            {
                TabloyuYenile();
                OdaKontrol();
                KalanGunleriAl();
                tcNoGiris.Focus();
                oncekiSecim = gizliButon;
                this.Size = new Size(712, 703);
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
                zaman.Text = DateTime.Now.ToString("dd.MM.yyyy - HH:mm");
                cikisTarihi.MaxDate = new DateTime(DateTime.Now.Year + 1, DateTime.Now.Month, DateTime.Now.Day);
                cikisTarihi.MinDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1);
            }
        }

        public void TabloyuYenile()
        {
            baglanti = new OleDbConnection($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dosyaYolu}");
            komut = new OleDbCommand(); ds = new DataSet();
            CheckForIllegalCrossThreadCalls = false;
            OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM Tablo1", baglanti);
            baglanti.Open();
            ds = new DataSet();
            adapter.Fill(ds, "Tablo1");
            baglanti.Close();
            dataGridView1.DataSource = ds.Tables["Tablo1"];
            toplamVeri.Text = $"Toplam Veri: {ToplamKullanici()}";

            WindowState = FormWindowState.Normal;
            Show(); BringToFront();
        }

        public void YeniKayitEkle(string TC_NO, string AD, string SOYAD, byte YAS, string CINSIYET, byte ODA_NO, string GIRIS_TARIHI, string CIKIS_TARIHI, byte KALAN_GUN, string TOPLAM_TUTAR, string DURUM)
        {
            baglanti.Open();
            komut = new OleDbCommand(string.Format($"INSERT INTO Tablo1 VALUES('{TC_NO}', '{AD}', '{SOYAD}', {YAS}, '{CINSIYET}', {ODA_NO}, '{GIRIS_TARIHI}', '{CIKIS_TARIHI}', {KALAN_GUN}, '{TOPLAM_TUTAR}', '{DURUM}')"), baglanti);
            try { komut.ExecuteNonQuery(); kayitBasarili = true; }
            catch (Exception hata) { MessageBox.Show("Yeni kayıt ekleme sırasında bir hata oluştu:\n"+ hata.Message, "Kayıt Eklenemedi", MessageBoxButtons.OK, MessageBoxIcon.Error); kayitBasarili = false; }
            baglanti.Close();
        }

        public void TabloyuGuncelle()
        {
            baglanti.Open();

            for (int i = 0; i < ToplamKullanici(); i++)
            {
                komut = new OleDbCommand(string.Format($"UPDATE Tablo1 SET AD='{Veri(i, 1)}', SOYAD='{Veri(i, 2)}', YAS={Veri(i, 3)}, CINSIYET='{Veri(i, 4)}', ODA_NO={Veri(i, 5)}, GIRIS_TARIHI='{Veri(i, 6)}', CIKIS_TARIHI='{Veri(i, 7)}', KALAN_GUN={Veri(i, 8)}, TOPLAM_TUTAR='{Veri(i, 9)}', DURUM='{Veri(i, 10)}' WHERE TC_NO='{Veri(i, 0)}'"), baglanti);
                try { komut.ExecuteNonQuery(); }
                catch (Exception hata) { MessageBox.Show("Kayıt güncelleme sırasında bir hata oluştu:\n" + hata.Message, "Kayıt Güncellenemedi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            baglanti.Close();
        }

        public string Veri(int satir, int sutun)
        {
            return dataGridView1.Rows[satir].Cells[sutun].Value.ToString();
        }

        public void VeriDegistir(int satir, int sutun, string deger)
        {
            dataGridView1.Rows[satir].Cells[sutun].Value = deger;
            TabloyuGuncelle();
        }

        public int ToplamKullanici()
        {
            if (dataGridView1.AllowUserToAddRows == true)
                return dataGridView1.Rows.Count - 1;
            else return dataGridView1.Rows.Count;
        }

        void KalanGunleriAl()
        {
            for (int i = 0; i < ToplamKullanici(); i++)
            {
                DateTime tarih;
                int oda = Convert.ToInt32(Veri(i, 5));
                cikisTarihleri[oda] = Veri(i, 7);
                if (Veri(i, 7) != "Belirsiz") tarih = Convert.ToDateTime(Veri(i, 7));
                else continue;
                int kalanGun = Convert.ToInt32(Math.Ceiling((tarih - DateTime.Now).TotalDays));
                dataGridView1.Rows[i].Cells[8].Value = kalanGun.ToString();
            }
        }

        private void TabloyuGuncelleButon_Click(object sender, EventArgs e)
        {
            tabloyuGuncelleButon.Enabled = false;
            TabloyuGuncelle();
        }

        private void OdaNumaralari(object sender, EventArgs e)
        {
            byte sira = 0;
            listView1.Items.Clear();
            Button b = (Button)sender;

            for (int i = 0; i < ToplamKullanici(); i++)
            {
                byte odaNo = Convert.ToByte(Veri(i, 5));
                byte oda = Convert.ToByte(b.Name.Substring(2));
                if (odaNo == oda)
                {
                    listView1.Items.Add(Veri(i, 0));
                    listView1.Items[sira].SubItems.Add(Veri(i, 1));
                    listView1.Items[sira].SubItems.Add(Veri(i, 2));
                    listView1.Items[sira].SubItems.Add(Veri(i, 6).Substring(0, 16));
                    listView1.Items[sira].SubItems.Add(Veri(i, 7));
                    sira++;
                }
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            tabloyuGuncelleButon.Enabled = true;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        void TutarHesapla()
        {
            cocukSayisi = 0;
            Button[] butonlar = { ikinciKisi, ucuncuKisi, dorduncuKisi };
            string[] degerler = { form2[6], form3[6], form4[6] };
            for (int i = 0; i < 3; i++)
            { 
                if (degerler[i] == "Ç" && butonlar[i].Enabled == true) cocukSayisi++; 
                kisiSayisiText.Text = string.Format($"{kisiSayisi} Kişi {cocukSayisi} Çocuk"); 
            }
            toplamTutar = odaFiyati * kalinacakGece * (kisiSayisi - cocukSayisi);
            if (belirsiz.Checked) 
                toplamTutarText.Text = "Günlük Tutar: " + toplamTutar + "₺"; 
            else toplamTutarText.Text = "Toplam Tutar: " + toplamTutar + "₺"; 
        }

        int odaFiyati = 0;
        int kisiSayisi = 1;
        int kalinacakGece = 1;
        int toplamTutar = 0;
        int cocukSayisi = 0;
        private void YeniKayit(object sender, EventArgs e)
        {
            HatalariSifirla();
            if (ODA_NO == 0) odaNoHata.Visible = true;
            if (!odaNoHata.Visible)
            {
                if (kisiSayisi == 1 && birinciKisi.Text != "1") SistemeKaydet(1);
                else if (kisiSayisi == 2 && birinciKisi.Text != "1" && ikinciKisi.Text != "2") SistemeKaydet(2);
                else if (kisiSayisi == 3 && birinciKisi.Text != "1" && ikinciKisi.Text != "2" && ucuncuKisi.Text != "3") SistemeKaydet(3);
                else if (kisiSayisi == 4 && birinciKisi.Text != "1" && ikinciKisi.Text != "2" && ucuncuKisi.Text != "3" && dorduncuKisi.Text != "4") SistemeKaydet(4);
                else gerekliBilgilerHata.Visible = true;
            }
        }

        bool kayitBasarili = false;
        string[] cikisTarihleri = new string[22];
        private void SistemeKaydet(byte kisiSayisi)
        {
            string DURUM;
            string TARIH = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            byte KALAN_GUN = Convert.ToByte(Math.Ceiling((cikisTarihi.Value - DateTime.Now).TotalDays));
            Array[] formlar = { form1, form2, form3, form4 };

            for (int i = 0; i < kisiSayisi; i++)
            {
                string TC_NO = formlar[i].GetValue(0).ToString();
                string AD = formlar[i].GetValue(1).ToString();
                string SOYAD = formlar[i].GetValue(2).ToString();
                byte YAS = Convert.ToByte(formlar[i].GetValue(3));
                string CINSIYET = formlar[i].GetValue(4).ToString(); 
                if (belirsiz.Checked) { cikisTarihleri[ODA_NO] = "Belirsiz"; DURUM = "Ödenmedi"; }
                else { cikisTarihleri[ODA_NO] = cikisTarihi.Value.ToString().Substring(0, 10); DURUM = "Ödendi"; }
                YeniKayitEkle(TC_NO, AD, SOYAD, YAS, CINSIYET, ODA_NO, TARIH, cikisTarihleri[ODA_NO].ToString(), KALAN_GUN, toplamTutar.ToString()+"₺", DURUM);
                if (kayitBasarili == true)
                {
                    MessageBox.Show("Kayıt Başarıyla Gerkeçleştirildi", "Kayıt Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TabloyuYenile();
                    FormuSifirla();
                    OdaKontrol();
                }
            }
        }

        private void FormuSifirla()
        {
            oncekiSecim.BackColor = Color.LightGreen;
            oncekiSecim = gizliButon;
            tcNoGiris.Text = "";
            adGiris.Text = "";
            soyadGiris.Text = "";
            birinciKisi.Text = "1";
            ikinciKisi.Text = "2";
            ucuncuKisi.Text = "3";
            dorduncuKisi.Text = "4";
            odaTutariText.Text = "Oda: 0";
            kisiSayisiText.Text = "1 Kişi 0 Çocuk";
            kalinacakGeceText.Text = "1 Gece";
            erkekSecenegi.Checked = false;
            kizSecenegi.Checked = false;
            odaNoGostergeci.Visible = false;
            belirsiz.Checked = false;
            bir.Checked = true;
            cikisTarihi.Value = DateTime.Now.AddDays(1);
            dogumTarihiSecimi.Value = Convert.ToDateTime("15.05.1990");
            oncekiSecim.BackColor = Color.LimeGreen;
            kisiSayisi = 1;
            seciliKisi = 1;
            ODA_NO = 0;
            Bir();
            Array[] formlar = { form1, form2, form3, form4 };
            for (int a = 0; a < 4; a++)
            {
                formlar[a].SetValue("", 0);
                formlar[a].SetValue("", 1);
                formlar[a].SetValue("", 2);
                formlar[a].SetValue("", 3);
                formlar[a].SetValue("", 4);
                formlar[a].SetValue("15.05.1990", 5);
                formlar[a].SetValue("Y", 6);
            }
            toplamTutarText.Text = "Toplam Tutar: 0₺";
        }

        byte ODA_NO = 0;
        private void OdaSecimleriniKaldir()
        {
            Button[] butonlar = { oda1, oda2, oda3, oda4, oda5, oda6, oda7, oda8, oda9,
            oda10, oda11, oda12, oda13, oda14, oda15, oda16, oda17, oda18, oda19, oda20 };
            for (int i = 0; i < 20; i++)
            {
                butonlar[i].BackColor = Color.LightGreen;
                butonlar[i].Enabled = true;
            }
        }

        Button oncekiSecim;
        private void OdaSecimi(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            oncekiSecim.BackColor = Color.LightGreen;
            b.BackColor = Color.LimeGreen;
            odaNoGostergeci.Visible = true;
            odaNoGostergeci.Text = b.Text;
            for (int i = 1; i <= 20; i++)
            {
                if (b.Name == $"oda{i}")
                {
                    ODA_NO = Convert.ToByte(i);
                    oncekiSecim = b;
                }
            }
            if (ODA_NO <= 8) odaFiyati = 150;
            else if (ODA_NO > 8 && ODA_NO <= 12) odaFiyati = 175;
            else odaFiyati = 200;
            odaTutariText.Text = string.Format($"Oda: {b.Text} - {odaFiyati}₺");
            TutarHesapla();
        }

        private void SadeceHarf(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) && !char.IsSeparator(e.KeyChar))
                e.Handled = true;
            if ((int)e.KeyChar == 32) e.Handled = true;
        }

        private void HatalariSifirla()
        {
            tcHata.Visible = false;
            adHata.Visible = false;
            soyadHata.Visible = false;
            cinsiyetHata.Visible = false;
            odaNoHata.Visible = false;
            gerekliBilgilerHata.Visible = false;
        }

        private void SeciliVeriyiSil_Click(object sender, EventArgs e)
        {
            string tcNumarasi = dataGridView1.SelectedCells[0].OwningRow.Cells[0].Value.ToString();
            string adi = dataGridView1.SelectedCells[0].OwningRow.Cells[1].Value.ToString();
            string soyadi = dataGridView1.SelectedCells[0].OwningRow.Cells[2].Value.ToString();
            string yasi = dataGridView1.SelectedCells[0].OwningRow.Cells[3].Value.ToString();
            string kayitTarihi = dataGridView1.SelectedCells[0].OwningRow.Cells[6].Value.ToString().Substring(0, 16);
            string odaNumarasi = dataGridView1.SelectedCells[0].OwningRow.Cells[5].Value.ToString();
            DialogResult secenek = MessageBox.Show($"Aşağıdaki kullanıcıyı sistemden kaldırmak istediğinize emin misiniz?\n\nTC Kimlik Numarası: {tcNumarasi}\nAdı: {adi}\nSoyadı: {soyadi}\nYaşı: {yasi}\nKayıt Tarihi: {kayitTarihi}\nOda Numarası: {odaNumarasi}", "Seçili Kullanıcıyı Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (secenek == DialogResult.Yes)
            {
                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedCells[0].RowIndex);
                baglanti.Open();
                komut = new OleDbCommand(string.Format($"DELETE FROM Tablo1"), baglanti);
                try { komut.ExecuteNonQuery(); }
                catch (Exception hata) { MessageBox.Show("Kayıt silme sırasında bir hata oluştu:\n" + hata.Message, "Kayıt Silinemedi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                for (int i = 0; i < ToplamKullanici(); i++)
                {
                    komut = new OleDbCommand(string.Format($"INSERT INTO Tablo1 VALUES('{Veri(i, 0)}', '{Veri(i, 1)}', '{Veri(i, 2)}', {Veri(i, 3)}, '{Veri(i, 4)}', {Veri(i, 5)}, '{Veri(i, 6)}', '{Veri(i, 7)}', {Veri(i, 8)}, '{Veri(i, 9)}', '{Veri(i, 10)}')"), baglanti);
                    try { komut.ExecuteNonQuery(); }
                    catch (Exception hata) { MessageBox.Show("Kayıt güncelleme sırasında bir hata oluştu:\n" + hata.Message, "Kayıt Güncellenemedi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
                baglanti.Close();
                TabloyuYenile();
                toplamVeri.Text = $"Toplam Veri: {ToplamKullanici()}";
            }
        }

        public void CikisKontrol()
        {
            KalanGunleriAl();
            listView2.Items.Clear();
            byte sira = 0;
            for (int i = 1; i <= 20; i++)
            {
                DateTime tarih;
                if (cikisTarihleri[i] != "Belirsiz" && cikisTarihleri[i] != "")
                    tarih = Convert.ToDateTime(cikisTarihleri[i]);
                else continue;

                int kalanGun = Convert.ToInt32(Math.Ceiling((tarih - DateTime.Now).TotalDays));
                if (kalanGun <= 0)
                {
                    for (int j = 0; j < ToplamKullanici(); j++)
                    {
                        int eslestirme = Convert.ToInt32(Veri(j, 5));
                        if (eslestirme == i)
                        {
                            listView2.Items.Add(Veri(j, 0));
                            listView2.Items[sira].SubItems.Add(Veri(j, 1));
                            listView2.Items[sira].SubItems.Add(Veri(j, 2));
                            listView2.Items[sira].SubItems.Add(Veri(j, 3));
                            listView2.Items[sira].SubItems.Add(Veri(j, 5));
                            listView2.Items[sira].SubItems.Add(Veri(j, 6).Substring(0, 16));
                            listView2.Items[sira].SubItems.Add(Veri(j, 9));
                            listView2.Items[sira].SubItems.Add(Veri(j, 10));
                            sira++;
                        }
                    }
                }
            }
        }

        private void OdaKontrol()
        {
            Button[] odalar = { oda1, oda2, oda3, oda4, oda5, oda6, oda7, oda8, oda9,
            oda10, oda11, oda12, oda13, oda14, oda15, oda16, oda17, oda18, oda19, oda20 };
            OdaSecimleriniKaldir();

            for (int i = 0; i < ToplamKullanici(); i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    if (Veri(i, 5).ToString() == odalar[j].Text)
                    {
                        odalar[j].BackColor = Color.Salmon;
                        odalar[j].Enabled = false;
                    }
                }
            }
        }

        private void NoKontrol()
        {
            Button[] nolar = { no1, no2, no3, no4, no5, no6, no7, no8, no9,
            no10, no11, no12, no13, no14, no15, no16, no17, no18, no19, no20 };

            for (int i = 0; i < ToplamKullanici(); i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    if (Veri(i, 5).ToString() == nolar[j].Text)
                    {
                        nolar[j].BackColor = Color.Salmon;
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            zaman.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }

        private void OdaNumarasiniGoster(object sender, MouseEventArgs e)
        {
            Button b = (Button)sender;
            odaNoGostergeci.Visible = true;
            odaNoGostergeci.Text = b.Text;
        }

        private void OdaNumarasiniGizle(object sender, EventArgs e)
        {
            if (ODA_NO == 0) odaNoGostergeci.Visible = false;
            else odaNoGostergeci.Text = ODA_NO.ToString();
        }

        private void KisiSayisi(object sender, EventArgs e)
        {
            RadioButton r = (RadioButton)sender;

            if (r.Name == "bir") Bir();
            else if (r.Name == "iki") Iki();
            else if (r.Name == "uc") Uc();
            else if (r.Name == "dort") Dort();

            seciliKisi = 1;
            KisiSecimi(birinciKisi, e);
            TutarHesapla();
        }
        void Bir()
        {
            birinciKisi.BackColor = Color.LimeGreen;

            ikinciKisi.Enabled = false;
            ucuncuKisi.Enabled = false;
            dorduncuKisi.Enabled = false;

            ikinciKisi.BackColor = Color.Salmon;
            ucuncuKisi.BackColor = Color.Salmon;
            dorduncuKisi.BackColor = Color.Salmon;
            iki.Checked = false;
            uc.Checked = false;
            dort.Checked = false;

            kisiSayisiText.Text = "1 Kişi 0 Çocuk";
            kisiSayisi = 1;
        }
        void Iki()
        {
            ikinciKisi.BackColor = Color.LightGreen;
            ikinciKisi.Enabled = true;
            iki.Checked = true;

            ucuncuKisi.BackColor = Color.Salmon;
            dorduncuKisi.BackColor = Color.Salmon;
            ucuncuKisi.Enabled = false;
            dorduncuKisi.Enabled = false;
            uc.Checked = false;
            dort.Checked = false;

            kisiSayisiText.Text = "2 Kişi 0 Çocuk";
            kisiSayisi = 2;
        }
        void Uc()
        {
            ikinciKisi.BackColor = Color.LightGreen;
            ucuncuKisi.BackColor = Color.LightGreen;
            ikinciKisi.Enabled = true;
            ucuncuKisi.Enabled = true;
            iki.Checked = true;
            uc.Checked = true;

            dorduncuKisi.BackColor = Color.Salmon;
            dorduncuKisi.Enabled = false;
            dort.Checked = false;

            kisiSayisiText.Text = "3 Kişi 0 Çocuk";
            kisiSayisi = 3;
        }
        void Dort()
        {
            ikinciKisi.BackColor = Color.LightGreen;
            ucuncuKisi.BackColor = Color.LightGreen;
            dorduncuKisi.BackColor = Color.LightGreen;
            ikinciKisi.Enabled = true;
            ucuncuKisi.Enabled = true;
            dorduncuKisi.Enabled = true;

            iki.Checked = true;
            uc.Checked = true;
            dort.Checked = true;

            kisiSayisiText.Text = "4 Kişi 0 Çocuk";
            kisiSayisi = 4;
        }

        byte seciliKisi = 1;
        private void KisiSecimi(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (b.Name == "birinciKisi")
            {
                tcNoGiris.Text = form1[0];
                adGiris.Text = form1[1];
                soyadGiris.Text = form1[2];
                if (form1[3] == "Kız") kizSecenegi.Checked = true;
                else if (form1[3] == "Erkek") erkekSecenegi.Checked = true;
                dogumTarihiSecimi.Value = Convert.ToDateTime(form1[5]);
                seciliKisi = 1;
            }
            else if (b.Name == "ikinciKisi")
            {
                tcNoGiris.Text = form2[0];
                adGiris.Text = form2[1];
                soyadGiris.Text = form2[2];
                if (form2[3] == "Kız") kizSecenegi.Checked = true;
                else if (form2[3] == "Erkek") erkekSecenegi.Checked = true;
                dogumTarihiSecimi.Value = Convert.ToDateTime(form2[5]);
                seciliKisi = 2;
            }
            else if (b.Name == "ucuncuKisi")
            {
                tcNoGiris.Text = form3[0];
                adGiris.Text = form3[1];
                soyadGiris.Text = form3[2];
                if (form3[3] == "Kız") kizSecenegi.Checked = true;
                else if (form3[3] == "Erkek") erkekSecenegi.Checked = true;
                dogumTarihiSecimi.Value = Convert.ToDateTime(form3[5]);
                seciliKisi = 3;
            }
            else if (b.Name == "dorduncuKisi")
            {
                tcNoGiris.Text = form4[0];
                adGiris.Text = form4[1];
                soyadGiris.Text = form4[2];
                if (form4[3] == "Kız") kizSecenegi.Checked = true;
                else if (form4[3] == "Erkek") erkekSecenegi.Checked = true;
                dogumTarihiSecimi.Value = Convert.ToDateTime(form4[5]);
                seciliKisi = 4;
            }
            Renklendir(sender);
        }

        void Renklendir(object sender)
        {
            Color[] renkler = { Color.LightGreen, Color.Salmon };
            Button b = (Button)sender;
            if (b.Name == "birinciKisi")
            {
                birinciKisi.BackColor = Color.LimeGreen;
                if (bir.Checked == true)
                {
                    ikinciKisi.BackColor = renkler[1];
                    ucuncuKisi.BackColor = renkler[1];
                    dorduncuKisi.BackColor = renkler[1];
                }
                else if (iki.Checked == true)
                {
                    ikinciKisi.BackColor = renkler[0];
                    ucuncuKisi.BackColor = renkler[1];
                    dorduncuKisi.BackColor = renkler[1];
                }
                else if (uc.Checked == true)
                {
                    ikinciKisi.BackColor = renkler[0];
                    ucuncuKisi.BackColor = renkler[0];
                    dorduncuKisi.BackColor = renkler[1];
                }
                else if (dort.Checked == true)
                {
                    ikinciKisi.BackColor = renkler[0];
                    ucuncuKisi.BackColor = renkler[0];
                    dorduncuKisi.BackColor = renkler[0];
                }
            }
            else if (b.Name == "ikinciKisi")
            {
                birinciKisi.BackColor = renkler[0];
                ikinciKisi.BackColor = Color.LimeGreen;
                if (iki.Checked == true)
                {
                    ucuncuKisi.BackColor = renkler[1];
                    dorduncuKisi.BackColor = renkler[1];
                }
                else if (uc.Checked == true)
                {
                    ucuncuKisi.BackColor = renkler[0];
                    dorduncuKisi.BackColor = renkler[1];
                }
                else if (dort.Checked == true)
                {
                    ucuncuKisi.BackColor = renkler[0];
                    dorduncuKisi.BackColor = renkler[0];
                }
            }
            else if (b.Name == "ucuncuKisi")
            {
                birinciKisi.BackColor = renkler[0];
                ucuncuKisi.BackColor = Color.LimeGreen;
                if (uc.Checked == true)
                {
                    ikinciKisi.BackColor = renkler[0];
                    dorduncuKisi.BackColor = renkler[1];
                }
                else if (dort.Checked == true)
                {
                    ikinciKisi.BackColor = renkler[0];
                    dorduncuKisi.BackColor = renkler[0];
                }
            }
            else if (b.Name == "dorduncuKisi" && dort.Checked == true)
            {
                birinciKisi.BackColor = renkler[0];
                ikinciKisi.BackColor = renkler[0];
                ucuncuKisi.BackColor = renkler[0];
                dorduncuKisi.BackColor = Color.LimeGreen;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int yas = DateTime.Now.Year - dogumTarihiSecimi.Value.Year;
            string TC_NO = tcNoGiris.Text;
            string ADI = adGiris.Text;
            string SOYADI = soyadGiris.Text;
            HatalariSifirla();
            if (TC_NO.Length < 11) tcHata.Visible = true;
            if (ADI.Length < 3) adHata.Visible = true;
            if (SOYADI.Length < 2) soyadHata.Visible = true;
            if (!erkekSecenegi.Checked && !kizSecenegi.Checked) cinsiyetHata.Visible = true;
            if (!tcHata.Visible && !adHata.Visible && !soyadHata.Visible && !cinsiyetHata.Visible)
            {
                if (seciliKisi == 1 && yas < 18)
                    MessageBox.Show("Ödeme yapacak kişi 18 yaşından küçük olamaz.", "Yaş Sınırı", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else FormuKaydet();

                TutarHesapla();
            }
        }

        string[] form1 = { "", "", "", "", "", "15.05.1990", "Y" };
        string[] form2 = { "", "", "", "", "", "15.05.1990", "Y" };
        string[] form3 = { "", "", "", "", "", "15.05.1990", "Y" };
        string[] form4 = { "", "", "", "", "", "15.05.1990", "Y" };
        void FormuKaydet()
        {
            int yas = DateTime.Now.Year - dogumTarihiSecimi.Value.Year;

            switch (seciliKisi)
            {
                case 1:
                    form1[0] = tcNoGiris.Text;
                    form1[1] = adGiris.Text;
                    form1[2] = soyadGiris.Text;
                    form1[3] = yas.ToString();
                    if (kizSecenegi.Checked == true) form1[4] = "Kız";
                    else if (erkekSecenegi.Checked == true) form1[4] = "Erkek";
                    form1[5] = dogumTarihiSecimi.Value.ToString().Substring(0, 10);
                    birinciKisi.Text = adGiris.Text + " " + soyadGiris.Text;
                    break;
                case 2:
                    form2[0] = tcNoGiris.Text;
                    form2[1] = adGiris.Text;
                    form2[2] = soyadGiris.Text;
                    form2[3] = yas.ToString();
                    if (kizSecenegi.Checked == true) form2[4] = "Kız";
                    else if (erkekSecenegi.Checked == true) form2[4] = "Erkek";
                    form2[5] = dogumTarihiSecimi.Value.ToString().Substring(0, 10);
                    ikinciKisi.Text = adGiris.Text + " " + soyadGiris.Text;
                    if (yas <= 6) form2[6] = "Ç"; else form2[6] = "Y";
                    break;
                case 3:
                    form3[0] = tcNoGiris.Text;
                    form3[1] = adGiris.Text;
                    form3[2] = soyadGiris.Text;
                    form3[3] = yas.ToString();
                    if (kizSecenegi.Checked == true) form3[4] = "Kız";
                    else if (erkekSecenegi.Checked == true) form3[4] = "Erkek";
                    form3[5] = dogumTarihiSecimi.Value.ToString().Substring(0, 10);
                    ucuncuKisi.Text = adGiris.Text + " " + soyadGiris.Text;
                    if (yas <= 6) form3[6] = "Ç"; else form3[6] = "Y";
                    break;
                case 4:
                    form4[0] = tcNoGiris.Text;
                    form4[1] = adGiris.Text;
                    form4[2] = soyadGiris.Text;
                    form4[3] = yas.ToString();
                    if (kizSecenegi.Checked == true) form4[4] = "Kız";
                    else if (erkekSecenegi.Checked == true) form4[4] = "Erkek";
                    form4[5] = dogumTarihiSecimi.Value.ToString().Substring(0, 10);
                    dorduncuKisi.Text = adGiris.Text + " " + soyadGiris.Text;
                    if (yas <= 6) form4[6] = "Ç"; else form4[6] = "Y";
                    break;
                default:
                    break;
            }
        }

        private void odaNoGostergeci_Click(object sender, EventArgs e)
        {
            tcNoGiris.Text = dogumTarihiSecimi.Value.ToString().Substring(0, 10);
            dogumTarihiSecimi.Value = Convert.ToDateTime(form4[4]);
        }

        private void cikisTarihi_ValueChanged(object sender, EventArgs e)
        {
            kalinacakGece = Convert.ToInt32(Math.Ceiling((cikisTarihi.Value - DateTime.Now).TotalDays));
            kalinacakGeceText.Text = kalinacakGece.ToString() + " Gece";
            TutarHesapla();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            toplamTutar = odaFiyati * kalinacakGece * (kisiSayisi - cocukSayisi);
            if (belirsiz.Checked)
            {
                toplamTutarText.Text = "Günlük Tutar: " + toplamTutar + "₺";
                kalinacakGeceText.Text = "1 Gece";
                kalinacakGece = 1;
                cikisTarihi.Enabled = false;
            }
            else
            {
                toplamTutarText.Text = "Toplam Tutar: " + toplamTutar + "₺";
                kalinacakGece = Convert.ToInt32(Math.Ceiling((cikisTarihi.Value - DateTime.Now).TotalDays));
                kalinacakGeceText.Text = kalinacakGece + " Gece";
                cikisTarihi.Enabled = true;
            }
            TutarHesapla();
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            seciliVeriyiSilButonu.Enabled = true;
        }

        private void zaman_Click(object sender, EventArgs e)
        {
            FormuSifirla();
        }

        private void cikisiniVer_Click(object sender, EventArgs e)
        {
            string tcNumarasi = listView2.SelectedItems[0].SubItems[0].Text;
            string adi = listView2.SelectedItems[0].SubItems[1].Text;
            string soyadi = listView2.SelectedItems[0].SubItems[2].Text;
            string girisTarihi = listView2.SelectedItems[0].SubItems[5].Text;
            string durum = listView2.SelectedItems[0].SubItems[7].Text;
            string odaNo = listView2.SelectedItems[0].SubItems[4].Text;
            DialogResult secenek = MessageBox.Show($"Aşağıdaki kullanıcıyı sistemden kaldırmak istediğinize emin misiniz?\n\nTC Kimlik Numarası: {tcNumarasi}\nAdı: {adi}\nSoyadı: {soyadi}\nGiriş Tarihi: {girisTarihi}\nÖdeme Durumu: {durum}\nOda Numarası: {odaNo}", "Seçili Kullanıcıyı Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (secenek == DialogResult.Yes)
            {
                for (int a = 0; a < ToplamKullanici(); a++)
                {
                    string tcNumarasiVT = dataGridView1.Rows[a].Cells[0].Value.ToString();
                    if (tcNumarasi == tcNumarasiVT)
                    {
                        dataGridView1.Rows.RemoveAt(a);
                        listView2.SelectedItems[0].Remove();
                        baglanti.Open();
                        komut = new OleDbCommand(string.Format($"DELETE FROM Tablo1"), baglanti);
                        try { komut.ExecuteNonQuery(); }
                        catch (Exception hata) { MessageBox.Show("Kayıt silme sırasında bir hata oluştu:\n" + hata.Message, "Kayıt Silinemedi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                        for (int i = 0; i < ToplamKullanici(); i++)
                        {
                            komut = new OleDbCommand(string.Format($"INSERT INTO Tablo1 VALUES('{Veri(i, 0)}', '{Veri(i, 1)}', '{Veri(i, 2)}', {Veri(i, 3)}, '{Veri(i, 4)}', {Veri(i, 5)}, '{Veri(i, 6)}', '{Veri(i, 7)}', {Veri(i, 8)}, '{Veri(i, 9)}', '{Veri(i, 10)}')"), baglanti);
                            try { komut.ExecuteNonQuery(); }
                            catch (Exception hata) { MessageBox.Show("Kayıt güncelleme sırasında bir hata oluştu:\n" + hata.Message, "Kayıt Güncellenemedi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                        }
                        baglanti.Close();
                        TabloyuYenile();
                        toplamVeri.Text = $"Toplam Veri: {ToplamKullanici()}";
                        break;
                    }
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                OdaKontrol();
                tcNoGiris.Focus();
                this.Size = new Size(712, 703);
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                TabloyuYenile();
                KalanGunleriAl();
                this.Size = new Size(1073, 703);
                dataGridView1.ClearSelection();
                tabloyuGuncelleButon.Enabled = false;
            }
            else if (tabControl1.SelectedIndex == 2)
            {
                CikisKontrol();
                this.Size = new Size(1073, 703);
            }
            else if (tabControl1.SelectedIndex == 3)
            {
                NoKontrol();
                this.Size = new Size(876, 703);
            }
            else if (tabControl1.SelectedIndex == 4)
            {
                YazdirmaTablosu();
                this.Size = new Size(1073, 703);
            }
        }

        void YazdirmaTablosu()
        {
            int sira = 0;
            listView3.Items.Clear();
            for (int i = 0; i < ToplamKullanici(); i++)
            {
                listView3.Items.Add(Veri(i, 0));
                for (int j = 1; j < 11; j++) listView3.Items[sira].SubItems.Add(Veri(i, j));

                sira++;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            listView3.MultiSelect = !listView3.MultiSelect;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView3.Items.Clear();
            int sira = 0;

            if (comboBox1.SelectedItem.ToString() != "Hepsi")
            {
                for (int i = 0; i < ToplamKullanici(); i++)
                {
                    if (Veri(i, 5) == comboBox1.SelectedItem.ToString())
                    {
                        listView3.Items.Add(Veri(i, 0));
                        for (int j = 1; j < 11; j++) listView3.Items[sira].SubItems.Add(Veri(i, j));

                        sira++;
                    }
                }
            }
            else YazdirmaTablosu();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView3.Items.Clear();
            int sira = 0;

            if (comboBox2.SelectedItem.ToString() != "Hepsi")
            {
                for (int i = 0; i < ToplamKullanici(); i++)
                {
                    if (Veri(i, 10) == comboBox2.SelectedItem.ToString())
                    {
                        listView3.Items.Add(Veri(i, 0));
                        for (int j = 1; j < 11; j++) listView3.Items[sira].SubItems.Add(Veri(i, j));

                        sira++;
                    }
                }
            }
            else YazdirmaTablosu();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            comboBox1.Enabled = true;
            comboBox2.Enabled = false;
            comboBox2.Text = "Hepsi";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = true;
            comboBox1.Enabled = false;
            comboBox1.Text = "Hepsi";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dokumanOnizle.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dokumanYazdir.Print();
        }

        Font YaziTipi(bool kalinlik = false, byte boyut = 15)
        {
            if (kalinlik == true) return new Font("Arial", boyut, FontStyle.Bold);
            else return new Font("Arial", boyut, FontStyle.Regular);
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            SolidBrush yazi = new SolidBrush(Color.Black);
            Pen cizgi = new Pen(Color.Black);
            e.Graphics.DrawString($"Tarih: {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}", YaziTipi(), yazi, 20, 35);
            e.Graphics.DrawString("Otel Müşteri Listesi", YaziTipi(true, 20), yazi, 300, 70);

            e.Graphics.DrawLine(cizgi, 20, 120, 805, 120);

            int[] sutunlar = { 30, 135, 230, 310, 350, 420, 460, 590, 0, 680, 740 };
            byte boyut = 10; int hiza = 150;
            e.Graphics.DrawString("TC No", YaziTipi(true, boyut), yazi, 30, hiza);
            e.Graphics.DrawString("Ad", YaziTipi(true, boyut), yazi, 135, hiza);
            e.Graphics.DrawString("Soyad", YaziTipi(true, boyut), yazi, 230, hiza);
            e.Graphics.DrawString("Yaş", YaziTipi(true, boyut), yazi, 310, hiza);
            e.Graphics.DrawString("Cinsiyet", YaziTipi(true, boyut), yazi, 350, hiza);
            e.Graphics.DrawString("Oda", YaziTipi(true, boyut), yazi, 420, hiza);
            e.Graphics.DrawString("Giriş Tarihi", YaziTipi(true, boyut), yazi, 460, hiza);
            e.Graphics.DrawString("Çıkış Tarihi", YaziTipi(true, boyut), yazi, 590, hiza);
            e.Graphics.DrawString("Tutar", YaziTipi(true, boyut), yazi, 680, hiza);
            e.Graphics.DrawString("Durum", YaziTipi(true, boyut), yazi, 740, hiza);

            int toplamVeri;
            if (checkBox2.Checked) toplamVeri = listView3.SelectedItems.Count;
            else toplamVeri = listView3.Items.Count;

            for (int i = 0; i < toplamVeri; i++)
            {
                hiza += 25;
                for (int j = 0; j < 11; j++)
                {
                    string veri;
                    if (j == 8) continue;
                    if (checkBox2.Checked) veri = listView3.SelectedItems[i].SubItems[j].Text;
                    else veri = listView3.Items[i].SubItems[j].Text;

                    e.Graphics.DrawString(veri, YaziTipi(false, boyut), yazi, sutunlar[j], hiza);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings1.Default.Save();
        }
    }
}