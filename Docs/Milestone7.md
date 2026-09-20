# M7 — Prosedürel labirent üretimi

M2–M6 kabul testleri kullanıcı tarafından onaylandı. M7 kodu ve veri testleri
tamamlandı. Bu milestone için Unity Play Mode, Android cihaz veya build testi
yapılmadı. M8 ve M9 başlatılmadı.

## Uygulama

- `LevelDefinition.source`: Authored (0) veya Generated (1).
- Authored bölümler mevcut TextAsset haritalarını kullanmaya devam eder.
- Generated bölümler `MazeGenerationSettings` verisini `MazeGenerator`'a verir.
- Her iki yol `LevelLayout` üretir; aynı MazeBuilder, kamera, Player, hedef,
  coin, skor ve kayıt sistemi kullanılır.
- İlk üç harita, ID ve sıra korunmuştur. Üç yeni bölüm sonlarına eklenmiştir.
  Eski üçüncü bölümü tamamlayan oyuncu, yeni dördüncü bölüme erişebilir.
- Menüde artan bölüm sayısı için dokunma/fare tekerleğiyle kaydırılabilir liste
  eklendi. Klavye seçimi görünüm dışında kalırsa liste seçili düğmeye kayar.

## Üretim ve garantiler

Üretici, oda merkezlerini iki kare aralıklarla yerleştirip iterative randomized
depth-first carving ile bütün odaları bağlar. Dış sınır kapalı kalır. Ardından
istenen sayıda iç duvar geçide dönüştürülür; yalnızca yol eklenir, yol kesilmez.

Başlangıç (1,1) oda merkezindedir. Ek geçitler açıldıktan sonra BFS ile gerçek
en kısa yol mesafeleri hesaplanır; en uzak oda merkezi hedef seçilir. Coinler
başlangıç/hedef dışındaki farklı oda merkezlerine dağıtılır. İstenen sayıda
coin için yer yoksa sayı sessizce azaltılmaz, ayar hatası bildirilir.

Sonuç, mevcut `LevelLayout` kontrolünden tekrar geçer: tek başlangıç ve hedef,
kapalı dış sınır, tanımlı karakterler ve tüm yürünebilir karelerin bağlantısı.
Geçersiz veri sahneye kurulmaz.

Seed, boyut, coin sayısı, ek geçit ve üretici sürümü aynıysa harita ve coin
konumları aynıdır. Restart veya uygulama yeniden açılması yeni harita seçmez.
Rastgelelik oyun sırasında global Unity Random durumuna bağlı değildir;
tanımlı 32-bit aritmetik kullanır. Android/IL2CPP üzerinde eşleşme bu çalışmada
çalıştırılarak doğrulanmadı.

Bu garantiler hücre verisi içindir. Unity fizik/collider/kamera davranışının
manuel kabulünü tek başına kanıtlamaz. Hücre boyu 1,4 birim ve mevcut oyuncu
boyutu korunmuştur; coinler dar bağlantılara veya duvar içine yerleştirilmez.

## Ayarlar

`Assets/Levels/GameConfig.asset` Inspector'ında ilgili bölümün Source alanını
Generated yapıp Generation alanlarını düzenleyebilirsiniz.

| Alan | Anlamı / sınırı |
|---|---|
| generatorVersion | Şimdilik 1; tanınmayan sürüm reddedilir |
| width / height | Dış sınır dahil kare sayısı; 5–25 arasında tek sayı |
| seed | Herhangi bir int; sıfır ve negatif değerler desteklenir |
| coinCount | 0 ile oda sayısı−2 arasında |
| extraConnections | Ağaç yapısına eklenecek geçit; 0 ile (oda sütunu−1)×(oda satırı−1) arasında |

Daha fazla ek geçit alternatif yol ve döngü sağlar; otomatik olarak daha zor
demek değildir. Boyut, seed ve yol uzunluğu birlikte değerlendirilmelidir.
25×25 sınırı mobilde kontrolsüz geometri artışını önler; bu boyutun cihazda
okunaklı veya performanslı olduğu iddia edilmez.

| Bölüm | ID | Boyut | Seed | Coin | Ek geçit | En kısa hedef yolu |
|---|---|---|---|---|---|---|
| 4 | maze-v1-forest | 11×9 | 4101 | 6 | 1 | 26 kare |
| 5 | maze-v1-crossing | 13×11 | 5206 | 8 | 2 | 48 kare |
| 6 | maze-v1-depths | 15×13 | 6303 | 10 | 1 | 62 kare |

Bu ölçümler veri testinden alınmıştır; oyun hissi/zorluk kabulü değildir.
Kamera bütün haritayı gösterdiği için büyük haritalarda oyuncu daha küçük
görünebilir; özellikle telefonda görünürlük kontrol edilmelidir.

## Kayıt ve sürümleme

Önceki bölüm kimlikleri, haritaları ve kayıt anahtarları değiştirilmedi.
Yeni bölümler yeni ID'ler kullanır. Yayınlanmış bir bölümün boyut/seed/coin
ayarını değiştirirken yeni ID kullanın veya açık bir kayıt göçü planlayın;
aksi halde eski best score farklı bir haritayla karşılaştırılır.

Üretici v1 için sabit SHA256 regresyon kontrolü vardır. İleride v1'in PRNG'sini
veya seçim sırasını sessizce değiştirmek yerine yeni sürüm eklenmelidir.
Coin konumları üretim sırasında seçilip sabit kalır; M8'deki süreli/rastgele
spawn sistemi bu milestone'a dahil değildir.

## Otomatik doğrulama

- Unity 2021.3.16f1 kütüphaneleriyle C# derleme.
- Önceki veri testleri ve genişleyen bölüm listesinin kayıt uyumluluğu.
- 8 boyut kombinasyonu × 128 seed = 1.024 konfigürasyon; coin ve ek geçit
  sayıları da değiştirilerek test edildi.
- Tekrarlanabilirlik, bağımsız flood fill ile erişim, dış duvar, tüm oda
  merkezlerinin varlığı, tam ek geçit sayısı, en uzak hedef, tam coin sayısı,
  benzersiz coin yerleşimi, S/G ile çakışmama kontrol edildi.
- Yalnızca seed değiştirilince harita çeşitliliği, minimum/maksimum ayarlar,
  int seed sınırları, yanlış parametre reddi, coin sayısının geometriyi
  değiştirmemesi ve v1 çıktı hash'i kontrol edildi.
- Testler gerçek GameConfig.asset içindeki üç generated bölümün ayarlarını
  okuyup ayrıca doğrular; yalnızca testteki örnek ayarları kullanmaz.
- Asset GUID/fileID/meta kontrolü ve git diff --check.

Tekrar çalıştırma:

```powershell
./Tools/Compile-Gameplay.ps1 M7
./Tools/Test-GameplayData.ps1
python Tools/Validate-Assets.py
```

## Manuel kabul listesi

1. MainMenu sahnesini aç, import tamamlanınca Play'e bas. Console'da hata
   olmamalı. Listeyi fare tekerleği veya sürükleyerek altıncı bölüme kadar kaydır.
2. M6 kayıtların varsa ilk üç bölümün best score değerlerini kontrol et.
   Önceden üçüncü bölümü bitirdiysen dördüncü bölüm açık olmalı; kayıtları silme.
3. Bölüm 4'ü başlat. Player, yeşil hedef ve altı coin görünmeli; duvarların
   içinde veya erişilemeyen alanda nesne olmamalı. Başlangıçtan hedefe ulaş.
4. Restart yap; harita, hedef ve coin başlangıç konumları aynı kalmalı.
   Bölüm skoru sıfırlanmalı, kalıcı best score korunmalı.
5. Menüye dönüp aynı bölümü aç ve uygulamayı yeniden açarak tekrarla;
   harita değişmemeli. Coin toplamadan bitirme de bölümü açmalı.
6. Bölüm 5 ve 6'ya ilerle: sırasıyla 8 ve 10 coin; yeni bölüm kilidi, sonuç,
   tekrar oynama ve son bölüm mesajı doğru olmalı.
7. Duvar köşeleri, dar bağlantılar, pause/resume ve respawn davranışını
   generated bölümlerde de kontrol et. Büyük haritada kamera tüm alanı göstermeli.
8. 16:9 / 20:9 / 4:3 Game View boyutlarında HUD, harita ve kaydırılabilir
   menünün görünümünü kontrol et. Klavye seçimi liste dışında kaybolmamalı.
9. Gerçek Android'de özellikle bölüm 6'nın oyuncu/coin görünürlüğünü,
   dokunarak liste kaydırmayı, tilt kontrolünü ve kare hızını değerlendir.

M7 için çalıştırılarak doğrulanmış bir Unity/Android runtime sonucu yoktur.
Otomatik veri kontrollerinde bilinen hata bulunmadı. M8'e geçilmedi.
