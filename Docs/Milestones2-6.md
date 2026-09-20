# M2–M6 uygulama ve kabul raporu

M1 Play Mode kabulü kullanıcı tarafından tamamlandı. M2–M6 kodu sırasıyla
uygulandı ve her milestone sonunda C# derlendi. M7 başlatılmadı.

**Bu rapor Unity Editor/Play Mode, Android cihaz veya APK build testi yapıldığı
anlamına gelmez.** Aşağıdaki otomatik kontroller Unity çalıştırılmadan yapıldı.

## Başlangıç

Unity 2021.3.16f1 ile `Assets/Scenes/MainMenu.unity` açılmalı. Build Settings
sırası MainMenu, Gameplay. Eski level0/level1 sahneleri karşılaştırma amaçlı
korunmuştur; yeni oyun akışının parçası değildir.

Yeni sahnelerde Edit Mode'da yalnızca bootstrap nesnesi bulunması normaldir.
UI ve harita, Play başladığında GameConfig üzerinden oluşturulur.

## Değişiklikler

### M2 — Player ve kamera

- Tek kaynak `Assets/prefabs/Player.prefab`; GUID korundu.
- Root: Rigidbody, CapsuleCollider, PlayerMotor, PlayerInput.
- Visual: MeshFilter, MeshRenderer ve görsel yön dönüşünü yapan PlayerVisual.
- Görsel model için Collider/Rigidbody eklemek gerekmiyor. Gelecek modelin
  Animator'ı Visual altında olmalı; root motion motorla ayrıca entegre edilmeden
  açılmamalı. Mevcut placeholder'ın Animator/animasyon asseti yok.
- MazeCamera eğik ortografik görünüm sağlar ve ekran oranı değişince haritayı
  yeniden kadrajlar. Fizik kökünü döndürmez.

### M3 — Ortak hareket motoru

- playerScript -> PlayerMotor; script GUID korunarak mevcut bağlantılar taşındı.
- Editor/masaüstü WASD/ok tuşları; destekleyen mobil cihazda accelerometer.
- LandscapeLeft; `Input.compensateSensors = true`. Ekran yönünü Unity
  telafi eder; uygulama aynı örneği ikinci kez döndürmez.
- Kalibrasyon: 0,2 saniye hazırlık + en az 0,75 saniye örnek ortalaması.
- Dead zone 0,07g; hassasiyet varsayılan 2,5; smoothing 0,12 saniye.
- Yatay azami hız 2,8 birim/sn; hızlanma 14, frenleme 20 birim/sn².
- C veya pause menüsü yeniden kalibrasyon yapar. Klavyede fiziksel sensör
  kalibrasyonu uygulanmaz. Hassasiyet yalnızca tilt girdisini etkiler.
- Kalibrasyon sırasında motor durdurulur. Pause sırasında kalibrasyon
  unscaled time ile tamamlanabilir.
- Sensörsüz mobil cihazda uyarı gösterilir; dokunmatik joystick eklenmedi.

Sensör yön telafisi için API kaynağı:
https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Input-compensateSensors.html
Bu API davranışını okumak, cihaz testi yerine geçmez.

### M4 — Oyun akışı

- MainMenu -> Gameplay -> hedef -> sonuç -> sonraki/tekrar/menü.
- GameSession: Loading, Playing, Paused, Won, Error durumları.
- LevelManager kurulum, MazeBuilder geometri, GoalTrigger hedef teması,
  PlayerRespawn sınır/düşme kontrolü yapar.
- Üç sabit harita: Level01.txt, Level02.txt, Level03.txt. Prosedürel değildir.
- LevelLayout tüm yürünebilir hücrelerin başlangıçtan erişilebildiğini,
  tek başlangıç/hedef olduğunu ve dış sınırın kapalı olduğunu kontrol eder.
- Esc/Android geri veya HUD düğmesi pause; odak kaybı/arka plana geçiş pause.
  Geri dönünce kullanıcı Devam et düğmesine basar.
- Y < -3 veya harita dış sınırından 1 birim taşma başlangıca geri getirir.
  Respawn mevcut coin/skoru korur; restart tüm bölüm oturumunu sıfırlar.
- UGUI arayüz, Türkçe metinler ve safe-area desteği eklendi.

### M5 — Coin/skor

- CoinSpawner doğrulanmış C hücrelerinin merkezlerine coin yerleştirir.
- pointScript -> Coin, GUID korunmuştur; eski sahne bağlantısı da geçerlidir.
- Coin oyuncuyu collider etiketi yerine bağlı Rigidbody/PlayerMotor ile tanır.
- ScoreService her coin ID'sini bir kez sayar. Coin başına 100 puan.
- HUD bölüm, alınan/toplam coin ve skoru; sonuç paneli bölüm sonucunu gösterir.
- Coinler kazanmak için zorunlu değildir. Rastgele/süreli coin M8 kapsamıdır.

### M6 — İlerleme/kayıt

- ProgressionService veri mantığı ile SaveService/PlayerPrefs depolaması ayrıldı.
- İlk bölüm açık; tamamlanan bölüm bir sonraki bölümü açar.
- Bölüm best score sadece daha yüksek sonuçla güncellenir; coinsiz kazanmak
  da geçerlidir. Tamamlanmadan çıkılan bölümün geçici skoru kaydedilmez.
- Kayıt anahtarları `RunnerMaze.v1.level.<sabit-id>.complete` ve `.best`.
  Bölüm isimleri değişebilir; yayın sonrası ID değişimi veri göçü gerektirir.
- Hassasiyet `RunnerMaze.v1.sensitivity` anahtarında saklanır.
- Menü kilitli bölümleri kapatır. Gameplay'e doğrudan girildiğinde de seçilen
  bölüm açılmış aralığa sınırlandırılır.
- Devam et en yüksek açılmış bölümü başlatır; yarım kalmış konumu yüklemez.
- Yeni build numarası değil, aynı uygulama kimliği kayıt devamlılığı için
  önemlidir. PlayerPrefs yereldir; cloud save, şifreleme veya ekonomi yoktur.

## Yapılan otomatik kontroller

- M2, M3, M4, M5, M6: Unity 2021.3.16f1 kütüphaneleriyle C# derleme.
- Son derlemede hata/uyarı yok.
- 26 saf veri kontrolü: üç gerçek haritanın erişilebilirliği, bozuk harita
  reddi, coin tekrar sayma/ID sınırı, skor reset, bölüm kilidi, sıfır coinle
  açılma, rekorun gerilememesi/yükselmesi, son bölüm sınırı, kayıt verisinden
  ilerleme yeniden oluşturma ve negatif skor savunması.
- Kayıt testleri bellek içi test deposu kullanır; gerçek PlayerPrefs diske
  yazma/uygulama yeniden açma testi yapılmadı.
- 27 serialized asset için GUID, fileID ve meta bütünlüğü kontrol edildi.
- `git diff --check` kontrolü.

Tekrar çalıştırma (proje kökünde PowerShell):

```powershell
./Tools/Compile-Gameplay.ps1 Review
./Tools/Test-GameplayData.ps1
python Tools/Validate-Assets.py
```

Derleme aracı yerel Unity 2021.3.16f1 kurulumunu ve Editor'ün oluşturduğu
Library/ScriptAssemblies/UnityEngine.UI.dll dosyasını kullanır. Temiz checkout'ta
önce Unity import işlemi gerekir. Bu araçlar tam Unity build testi değildir.

## Unity Editor manuel kabul listesi

1. Unity'nin import/compile işleminin bitmesini bekle. Console'u temizle;
   MainMenu sahnesini aç ve Play'e bas. Missing Script/import hatası olmamalı.
2. İlk kayıtta yalnızca bölüm 1 açık olmalı. Devam et bölüm 1'i açmalı.
   Gameplay'e geçişte tek Player, tek kamera ve tek EventSystem olmalı.
3. Player Root altında Rigidbody/Collider/scriptler; Visual altında görsel
   bulunmalı. Visual ölçeğini geçici değiştirmek root collider'ını değiştirmemeli.
4. WASD ve ok tuşlarıyla dört yönde hareket et. Diyagonal hız artmamalı;
   tuşları bırakınca kontrollü durmalı. Space zıplatmamalı.
5. Duvarlara düz/çapraz yürü, köşeleri dön. Duvarın içinden geçme veya takılı
   kalma olmamalı. Rigidbody'nin yatay hız büyüklüğü 2,8'i aşmamalı.
6. Coin topla: HUD coin sayısı bir, skor 100 artmalı; aynı noktadan tekrar
   geçmek skor eklememeli. Coin toplamadan da hedefe ulaşılabilmeli.
7. Esc ve Duraklat düğmesini dene. Hareket/coin animasyonu durmalı. Devam et
   sonrası oyun sürmeli; aynı tuşla pause/resume tekrarı sorun çıkarmamalı.
8. Pause'da yeniden başlat: başlangıç konumu, coinler ve bölüm skoru reset;
   önceki kazanımlardan gelen best score/level kilitleri korunmalı.
9. Respawn denemesi: Play Mode'da Player root Y konumunu -4 yap; gerekiyorsa
   Game View'a dönüp Devam et. Oyuncu başlangıca gelmeli; mevcut skor korunmalı.
10. Hedefe gir: kazanma paneli bir kez açılmalı; oyuncu durmalı. HUD/sonuç
    coin ve skor değerleri eşleşmeli. Sonraki bölüm ve Tekrar oyna denenmeli.
11. Ana menüye dön: sonraki bölüm açık, best score görünür olmalı. Daha düşük
    skorla tekrar kazanmak rekoru düşürmemeli; daha yüksek skor yükseltmeli.
12. Üçüncü bölümü bitir: tamamlanma mesajı gösterilmeli; dördüncü bölüme
    gitmeye çalışan düğme/indeks hatası olmamalı.
13. Play'i durdurup tekrar başlat, ayrıca Editor'ü kapat/aç: açık bölümler,
    best score ve değiştirilen hassasiyet korunmalı. Editör ve Android
    kayıtlarının farklı ortamlarda olduğunu unutma.
14. Game View'u 16:9, 20:9 ve 4:3 boyutlarında kontrol et: harita kadrajda,
    HUD okunur, düğmeler erişilebilir, pause/sonuç paneli taşmıyor olmalı.
15. Menü/oyun/restart arasında birkaç tur geçiş yap: yinelenen kamera,
    EventSystem veya AudioListener uyarısı olmamalı. Console'u tekrar kontrol et.

## Gerçek Android cihaz kabul listesi

1. Android build/import işlemini ayrıca doğrula. Bu çalışmada APK üretilmedi.
2. LandscapeLeft görünümde cihazı rahat tut ve başlangıç kalibrasyonunun
   bitmesini bekle. Nötr durumda belirgin kayma olmamalı.
3. Sağ/sol ve ileri/geri eğ: yönler ekrandaki hareketle eşleşmeli. Sensör
   eksen işareti ve tutuş rahatlığı masaüstü testinden çıkarılamaz.
4. Küçük titreşimler dead zone ile bastırılmalı; büyük eğim hız sınırını
   aşmamalı. Gecikme/hassasiyeti pause ayarıyla değerlendir.
5. Farklı tutuşta pause -> yeniden kalibre et -> devam et; yeni nötr konum
   kullanılmalı. Hassasiyet uygulama yeniden açıldığında korunmalı.
6. Uygulamayı arka plana al/ekranı kilitle/geri dön. Pause açık kalmalı;
   oyuncu arka planda ilerlememeli. Gerekirse yeniden kalibre et.
7. Dokunmatik tüm düğmeler, çentik/safe area, bölüm geçişi, tekrar oynama ve
   uygulamayı tamamen kapatıp açtıktan sonra kayıt devamlılığı doğrulanmalı.

## Riskler ve M7 öncesi koşullar

- Bilinen derleme veya statik referans hatası yok. Runtime doğrulaması yok;
  UI yerleşimi, sahne yaşam döngüsü ve fizik için yukarıdaki liste gerekli.
- Mobil tilt yönü, smoothing/hassasiyet ve başlangıç kalibrasyonu gerçek
  cihazda ayar isteyebilir. Telefonun dik tutulması yerine rahat, kısmen
  yatay tutuş hedeflendi.
- Eski Android Gradle/manifest şablonları değiştirilmedi; build zinciri
  uyumluluğu ve mağaza koşulları M10'da ayrıca ele alınmalı.
- Görseller placeholder. Ses, Animator klipleri, hareketli duvar, rastgele
  coin ve prosedürel üretim eklenmedi.
- UGUI kodla kuruluyor; harita ve UI Edit Mode'da önizlenmiyor. Harita ekleme
  noktası Assets/Levels/GameConfig.asset. Harita dili: # duvar, . yol,
  S başlangıç, G hedef, C coin. Yeni map, GameConfig listesine eklenir.
- M7 öncesinde yeni akışın Editor kabulü, kayıt yeniden açma testi ve en az
  bir gerçek cihaz tilt testi tamamlanmalı. Bu testlerin bulduğu sorunlar
  giderilmeden harita üretim çeşitliliğini artırmak önerilmez.
- M7 için yeniden mimari yazmak gerekmiyor: üretici LevelLayout eşdeğeri
  yürünebilir veri sağlayabilir; builder/coin/score/progression korunabilir.
