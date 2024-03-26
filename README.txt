Yesilcam Film Sistemi Kod İncelemesi
Proje Genel Bakış
Yesilcam Film Sistemi, Yeşilçam film verilerini yönetmek ve izlemek amacıyla geliştirilmiş bir Windows Forms uygulamasıdır. PostgreSQL veritabanını kullanarak film, oyuncu, yönetmen ve film türü verilerini depolar ve kullanıcılara erişim sağlar.

Anahtar Özellikler
Film Yönetimi

Filmleri listeleme, ekleme, güncelleme ve silme özellikleri bulunmaktadır.
Oyuncular, yönetmenler ve film türleri ile ilişkilendirilmiş filmleri görüntüleme.
Oyuncu Yönetimi

Oyuncuları listeleme, ekleme, güncelleme ve silme özellikleri bulunmaktadır.
Oyuncuların oynadığı filmleri listeleyebilme.
Yönetmen Yönetimi

Yönetmenleri listeleme, ekleme, güncelleme ve silme özellikleri bulunmaktadır.
Yönetmenlerin çektiği filmleri ve aldığı ödülleri listeleyebilme.
Film Türü Yönetimi

Film türlerini listeleme, ekleme, güncelleme ve silme özellikleri bulunmaktadır.


Bağlantı Yönetimi

NpgsqlConnection sınıfı kullanılarak PostgreSQL veritabanına bağlantı sağlanmıştır.
Bağlantı durumu kontrol edilerek hata durumları yönetilmiştir.

Form İşlemleri

Ana form (Form1) üzerinde film, oyuncu, yönetmen ve film türü listeleri gösterilmiştir.
Zamanlayıcı kullanılarak belirli aralıklarla bir etiketin görünürlüğü değiştirilmiştir.

Veritabanı İşlemleri

Film, oyuncu, yönetmen ve film türü verileri için ayrı ayrı listeleme, ekleme, güncelleme ve silme işlemleri yapılmıştır.
Veri girişi sırasında doğrulama kontrolleri yapılmıştır.

Film İşlemleri

Filmlerin oyuncuları, yönetmeni ve türü ile ilişkilendirilmiş bilgileri listeleme ve güncelleme işlemleri yapılmıştır.
Film listesini gösteren DataGridView kontrolü kullanılmıştır.
Film afişleri ise Afisler adındaki klasörde saklanmaktadır.

Oyuncu İşlemleri

Oyuncuların oynadığı filmleri listeleme ve güncelleme işlemleri yapılmıştır.
Oyuncu listesini gösteren DataGridView kontrolü kullanılmıştır.

Yönetmen İşlemleri

Yönetmenlerin çektiği filmleri ve aldığı ödülleri listeleme işlemleri yapılmıştır.
Yönetmen listesini gösteren DataGridView kontrolü kullanılmıştır.

Film Türü İşlemleri

Film türlerini listeleme, ekleme, güncelleme ve silme işlemleri yapılmıştır.
Film türü listesini gösteren DataGridView kontrolü kullanılmıştır.