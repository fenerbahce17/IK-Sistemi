using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PersonelSystem; // EDMX namespace


namespace PersonelSystem.Controllers
{
    public class PersonelController : Controller
    {
        private PersonelBilgiFormu2Entities1 db = new PersonelBilgiFormu2Entities1(); // EDMX context adı

        public ActionResult Index()
        {
            var personeller = db.Personel.ToList();

            return View(personeller);
        }
        [HttpPost]
        public JsonResult DeleteAjax(int id)
        {
            try
            {
                var personel = db.Personel
                                 .Include("OkulBilgisi")
                                 .Include("IsBilgisi")
                                 .Include("Belgeler")
                                 .FirstOrDefault(x => x.PersonelId == id);

                if (personel == null)
                    return Json(new { success = false, message = "Kayıt bulunamadı." });

                db.OkulBilgisi.RemoveRange(personel.OkulBilgisi);
                db.IsBilgisi.RemoveRange(personel.IsBilgisi);
                db.Belgeler.RemoveRange(personel.Belgeler);
                db.Personel.Remove(personel);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteSelected(List<int> ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    var personel = db.Personel
                                     .Include("OkulBilgisi")
                                     .Include("IsBilgisi")
                                     .Include("Belgeler")
                                     .FirstOrDefault(x => x.PersonelId == id);

                    if (personel != null)
                    {
                        db.OkulBilgisi.RemoveRange(personel.OkulBilgisi);
                        db.IsBilgisi.RemoveRange(personel.IsBilgisi);
                        db.Belgeler.RemoveRange(personel.Belgeler);
                        db.Personel.Remove(personel);
                    }
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Personel model, HttpPostedFileBase Resim, IEnumerable<HttpPostedFileBase> PdfFiles, string action)
        {
            if (action == "preview")
            {
                TempData["PreviewModel"] = model;
                TempData["OkulAdlari"] = Request.Form.GetValues("OkulAdi[]");
                TempData["Bolumler"] = Request.Form.GetValues("Bolum[]");
                TempData["Yillar"] = Request.Form.GetValues("Yil[]");

                TempData["Firmalar"] = Request.Form.GetValues("Firma[]");
                TempData["Pozisyonlar"] = Request.Form.GetValues("Pozisyon[]");
                TempData["YilIs"] = Request.Form.GetValues("YilIs[]");

                return RedirectToAction("Preview");
            }

            // ✅ Boş alan kontrolü (Ad, Soyad, DogumTarihi zorunlu)
            if (string.IsNullOrWhiteSpace(model.Ad) ||
                string.IsNullOrWhiteSpace(model.Soyad) ||
                string.IsNullOrWhiteSpace(model.Adres) ||
                string.IsNullOrWhiteSpace(model.Telefon) ||
                string.IsNullOrWhiteSpace(model.MedeniDurum) ||
                string.IsNullOrWhiteSpace(model.Email) ||




                model.DogumTarihi == null)
            {
                ViewBag.Mesaj = "❌ Lütfen eksik  alanlarını doldurunuz.";
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (Resim != null && Resim.ContentLength > 0)
                    {
                        string dosyaAdi = Guid.NewGuid() + System.IO.Path.GetExtension(Resim.FileName);
                        string kayitYolu = Server.MapPath("~/Uploads/" + dosyaAdi);

                        if (!System.IO.Directory.Exists(Server.MapPath("~/Uploads")))
                            System.IO.Directory.CreateDirectory(Server.MapPath("~/Uploads"));

                        Resim.SaveAs(kayitYolu);
                        model.ResimYolu = "/Uploads/" + dosyaAdi;
                    }


                    db.Personel.Add(model);
                    db.SaveChanges();

                    // Okul Bilgileri
                    string[] okulAdlari = Request.Form.GetValues("OkulAdi[]");
                    string[] bolumler = Request.Form.GetValues("Bolum[]");
                    string[] mezunYillari = Request.Form.GetValues("Yil[]");

                    if (okulAdlari != null)
                    {
                        for (int i = 0; i < okulAdlari.Length; i++)
                        {
                            // Boşluk kontrolü ekle
                            if (string.IsNullOrWhiteSpace(okulAdlari[i]) && string.IsNullOrWhiteSpace(bolumler[i]) && string.IsNullOrWhiteSpace(mezunYillari[i]))
                                continue;

                            if (string.IsNullOrWhiteSpace(okulAdlari[i]) || string.IsNullOrWhiteSpace(bolumler[i]))
                                continue;

                            var okul = new OkulBilgisi
                            {
                                PersonelId = model.PersonelId,
                                OkulAdi = okulAdlari[i],
                                Bolum = bolumler[i],
                                MezuniyetYili = string.IsNullOrEmpty(mezunYillari[i]) ? (int?)null : int.Parse(mezunYillari[i])
                            };
                            db.OkulBilgisi.Add(okul);
                        }
                    }

                    // İş Bilgileri
                    string[] firmalar = Request.Form.GetValues("Firma[]");
                    string[] pozisyonlar = Request.Form.GetValues("Pozisyon[]");
                    string[] yillar = Request.Form.GetValues("YilIs[]");

                    if (firmalar != null)
                    {
                        for (int i = 0; i < firmalar.Length; i++)
                        {
                            // Boşluk kontrolü ekle
                            if (string.IsNullOrWhiteSpace(firmalar[i]) && string.IsNullOrWhiteSpace(pozisyonlar[i]) && string.IsNullOrWhiteSpace(yillar[i]))
                                continue;

                            if (string.IsNullOrWhiteSpace(firmalar[i]) || string.IsNullOrWhiteSpace(pozisyonlar[i]))
                                continue;

                            var isBilgisi = new IsBilgisi
                            {
                                PersonelId = model.PersonelId,
                                FirmaAdi = firmalar[i],
                                Gorev = pozisyonlar[i],
                                CalismaYili = string.IsNullOrEmpty(yillar[i]) ? (int?)null : int.Parse(yillar[i])
                            };
                            db.IsBilgisi.Add(isBilgisi);
                        }
                    }

                    // Belgeler (PDF)
                    if (PdfFiles != null)
                    {
                        foreach (var pdf in PdfFiles)
                        {
                            if (pdf != null && pdf.ContentLength > 0)
                            {
                                string pdfAdi = Guid.NewGuid() + System.IO.Path.GetExtension(pdf.FileName);
                                string pdfYol = Server.MapPath("~/Uploads/" + pdfAdi);
                                pdf.SaveAs(pdfYol);

                                var belge = new Belgeler
                                {
                                    PersonelId = model.PersonelId,
                                    DosyaAdi = pdf.FileName,
                                    DosyaYolu = "/Uploads/" + pdfAdi
                                };
                                db.Belgeler.Add(belge);
                            }
                        }
                    }

                    db.SaveChanges();

                    TempData["Mesaj"] = "✅ Personel ve ek bilgiler başarıyla kaydedildi.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Mesaj = "❌ Hata: " + ex.Message;
                }
            }

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = db.Personel
                  .Include("OkulBilgisi")
                  .Include("IsBilgisi")
                  .Include("Belgeler")
                  .FirstOrDefault(x => x.PersonelId == id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(Personel model, HttpPostedFileBase Resim, IEnumerable<HttpPostedFileBase> PdfFiles)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var personel = db.Personel.Find(model.PersonelId);
                if (personel == null) return HttpNotFound();

                // ✅ Fotoğraf güncelleme
                if (Resim != null && Resim.ContentLength > 0)
                {
                    string dosyaAdi = Guid.NewGuid() + System.IO.Path.GetExtension(Resim.FileName);
                    string kayitYolu = Server.MapPath("~/Uploads/" + dosyaAdi);

                    if (!System.IO.Directory.Exists(Server.MapPath("~/Uploads")))
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Uploads"));

                    Resim.SaveAs(kayitYolu);
                    personel.ResimYolu = "/Uploads/" + dosyaAdi;
                }

                // ✅ Kişisel bilgiler
                personel.Ad = model.Ad;
                personel.Soyad = model.Soyad;
                personel.DogumTarihi = model.DogumTarihi;
                personel.Cinsiyet = model.Cinsiyet;
                personel.MedeniDurum = model.MedeniDurum;
                personel.Adres = model.Adres;
                personel.Telefon = model.Telefon;
                personel.Email = model.Email;

                // ✅ Eski okul bilgilerini sil
                var eskiOkullar = db.OkulBilgisi.Where(x => x.PersonelId == model.PersonelId).ToList();
                db.OkulBilgisi.RemoveRange(eskiOkullar);

                // ✅ Yeni okul bilgilerini al ve ekle
                string[] okulAdlari = Request.Form.GetValues("OkulAdi[]");
                string[] bolumler = Request.Form.GetValues("Bolum[]");
                string[] mezunYillari = Request.Form.GetValues("Yil[]");

                if (okulAdlari != null && bolumler != null && mezunYillari != null)
                {
                    int max = Math.Max(okulAdlari.Length, Math.Max(bolumler.Length, mezunYillari.Length));
                    for (int i = 0; i < max; i++)
                    {
                        var okul = i < okulAdlari.Length ? okulAdlari[i]?.Trim() : "";
                        var bolum = i < bolumler.Length ? bolumler[i]?.Trim() : "";
                        var yil = i < mezunYillari.Length ? mezunYillari[i]?.Trim() : "";

                        // Boş satır tamamen atla
                        if (string.IsNullOrWhiteSpace(okul) && string.IsNullOrWhiteSpace(bolum) && string.IsNullOrWhiteSpace(yil))
                            continue;

                        // Zorunlu alan kontrolü
                        if (string.IsNullOrWhiteSpace(okul) || string.IsNullOrWhiteSpace(bolum))
                            continue;

                        db.OkulBilgisi.Add(new OkulBilgisi
                        {
                            PersonelId = model.PersonelId,
                            OkulAdi = okul,
                            Bolum = bolum,
                            MezuniyetYili = string.IsNullOrEmpty(yil) ? (int?)null : int.Parse(yil)
                        });
                    }
                }


                // ✅ Eski iş bilgilerini sil
                var eskiIsler = db.IsBilgisi.Where(x => x.PersonelId == model.PersonelId).ToList();
                db.IsBilgisi.RemoveRange(eskiIsler);

                // ✅ Yeni iş bilgilerini al ve ekle
                string[] firmalar = Request.Form.GetValues("Firma[]");
                string[] pozisyonlar = Request.Form.GetValues("Pozisyon[]");
                string[] yillar = Request.Form.GetValues("YilIs[]");

                if (firmalar != null && pozisyonlar != null && yillar != null)
                {
                    int max = Math.Max(firmalar.Length, Math.Max(pozisyonlar.Length, yillar.Length));
                    for (int i = 0; i < max; i++)
                    {
                        var firma = i < firmalar.Length ? firmalar[i]?.Trim() : "";
                        var pozisyon = i < pozisyonlar.Length ? pozisyonlar[i]?.Trim() : "";
                        var yil = i < yillar.Length ? yillar[i]?.Trim() : "";

                        // Boş satır tamamen atla
                        if (string.IsNullOrWhiteSpace(firma) && string.IsNullOrWhiteSpace(pozisyon) && string.IsNullOrWhiteSpace(yil))
                            continue;

                        // Zorunlu alan kontrolü
                        if (string.IsNullOrWhiteSpace(firma) || string.IsNullOrWhiteSpace(pozisyon))
                            continue;

                        db.IsBilgisi.Add(new IsBilgisi
                        {
                            PersonelId = model.PersonelId,
                            FirmaAdi = firma,
                            Gorev = pozisyon,
                            CalismaYili = string.IsNullOrEmpty(yil) ? (int?)null : int.Parse(yil)
                        });
                    }
                }

                // ✅ Yeni PDF belgeleri ekle
                if (PdfFiles != null)
                {
                    foreach (var pdf in PdfFiles)
                    {
                        if (pdf != null && pdf.ContentLength > 0)
                        {
                            string pdfAdi = Guid.NewGuid() + System.IO.Path.GetExtension(pdf.FileName);
                            string pdfYolu = Server.MapPath("~/Uploads/" + pdfAdi);
                            pdf.SaveAs(pdfYolu);

                            db.Belgeler.Add(new Belgeler
                            {
                                PersonelId = model.PersonelId,
                                DosyaAdi = pdf.FileName,
                                DosyaYolu = "/Uploads/" + pdfAdi
                            });
                        }
                    }
                }
                var silinecekBelgeler = Request.Form.GetValues("SilinecekBelgeler");
                if (silinecekBelgeler != null)
                {
                    foreach (var id in silinecekBelgeler)
                    {
                        int belgeId = int.Parse(id);
                        var belge = db.Belgeler.Find(belgeId);
                        if (belge != null)
                        {
                            // Dosya varsa fiziksel olarak sil
                            var fizikselYol = Server.MapPath(belge.DosyaYolu);
                            if (System.IO.File.Exists(fizikselYol))
                                System.IO.File.Delete(fizikselYol);

                            db.Belgeler.Remove(belge);
                        }
                    }
                }


                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Mesaj = "❌ Hata: " + ex.Message;
                return View(model);
            }
        }


        [HttpPost]
        public JsonResult BelgeSil(int id)
        {
            try
            {
                var belge = db.Belgeler.Find(id);
                if (belge == null)
                    return Json(new { success = false, message = "Belge bulunamadı." });

                // Fiziksel dosyayı sil
                var fizikselYol = Server.MapPath(belge.DosyaYolu);
                if (System.IO.File.Exists(fizikselYol))
                    System.IO.File.Delete(fizikselYol);

                // Veritabanından sil
                db.Belgeler.Remove(belge);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




        public ActionResult Preview()
        {
            var model = TempData["PreviewModel"] as Personel;
            ViewBag.OkulAdlari = TempData["OkulAdlari"] as string[];
            ViewBag.Bolumler = TempData["Bolumler"] as string[];
            ViewBag.Yillar = TempData["Yillar"] as string[];

            ViewBag.Firmalar = TempData["Firmalar"] as string[];
            ViewBag.Pozisyonlar = TempData["Pozisyonlar"] as string[];
            ViewBag.YilIs = TempData["YilIs"] as string[];
         

            TempData.Keep();

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
  
