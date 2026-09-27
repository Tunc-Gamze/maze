# M8 — Süreli coinler

M8 kodu eklendi. Unity Editor Play Mode, Android cihaz veya APK build testi
yapılmadı. M9 hareketli duvar sistemine geçilmedi.

## Kapsam ve kayıt uyumluluğu

Önceki altı bölümün haritası, coin davranışı, kimliği ve best score kayıtları
korundu. Süreli coinler yeni 7. ve 8. bölümlere eklendi. Altıncı bölümü daha
önce tamamlayan oyuncu yedinci bölümü açılmış görür. Coin toplamak zorunlu
değildir; hedefe ulaşmak aynı şekilde bölümü tamamlar.

| Bölüm | Toplam coin hakkı | Aynı anda en fazla | Spawn aralığı | Ömür | Uyarı |
|---|---|---|---|---|---|
| 7 — Altın Fırsatlar | 10 | 3 | 6 sn | 30 sn | Son 5 sn |
| 8 — Zamanlı Hazine | 12 | 4 | 5 sn | 26 sn | Son 5 sn |

İlk spawn, aktif oynanışın ikinci saniyesinde denenir. Coin başına 100 puan;
bu iki bölümün azami skorları 1000 ve 1200'dür. Süresi dolan coin hakkı
yenilenmez. Bekleyerek sınırsız coin/skor elde edilemez.

## Sistemler

- `CoinSpawnSettings`: static/timed mod, ayrı seed, toplam hak, aktif sınır,
  ilk gecikme, aralık, ömür ve uyarı ayarları. Yanlış/sonsuz/NaN değerler reddedilir.
- `TimedCoinCycle`: Unity bağımsız zaman, sıralama, aktif coin ve toplam hak
  takibi. Her ortaya çıkış farklı bir ID alır; aynı konum tekrar kullanılabilir
  ama aynı aktif coin ikinci kez puan veremez.
- `CoinSpawner`: veri modelini sahne nesnelerine bağlar. Süresi dolan/toplanan
  coinleri hemen etkisizleştirir ve yok eder. Static bölümlerin davranışı korunur.
- `Coin`: dönüş ve son saniyelerde yanıp sönme. Uyarı sadece renderer'ı etkiler;
  coin gerçek süre dolana kadar toplanabilir. Kalibrasyonda temas eden oyuncu,
  kontrol geri geldiğinde OnTriggerStay üzerinden de coin toplayabilir.
- `GameSession`: skoru yalnızca halen canlı ve geçerli coin ID'si için artırır.
- HUD: alınan/toplam hak, skor, aktif, sırada, süresi dolan ve en yakın coin
  bitiş süresi. Sonuçta kaçan ve toplam toplanmayan coin sayıları gösterilir.

## Güvenli konumlar ve deterministik sıra

Spawn adayları, LevelLayout erişilebilirlik kontrolünden geçmiş C hücreleridir.
Yeni generated bölümlerde bunlar oda merkezleridir; duvar, başlangıç ve hedef
değildir. Bölüm 7'nin haritasında 12, bölüm 8'de 16 aday nokta vardır; bu sayılar
toplam coin hakkından ayrıdır. Harita seed'i ile coin sırası seed'i ayrıdır.

Adaylar seed ile bir kez karıştırılır ve gerekiyorsa aynı sırayla tekrar
dolaşılır. Spawn sırası, pause veya güvenlik gecikmesinde değişmez. Restart'ta
aynı sıra baştan başlar.

Sıradaki konumda aktif coin varsa, oyuncunun yatay uzaklığı 1 birimden azsa
veya 0,22 birim yarıçaplı fizik kontrolü bir collider/trigger bulursa spawn
ertelenir. Hak tüketilmez, başka konuma rastgele atlanmaz. Konum güvenli
olunca aynı coin doğar. Bu yüzden **aynı seed aynı sırayı sağlar; oyuncunun
hareketi nedeniyle gerçek doğum zamanları farklılaşabilir.**

Bir karede en fazla bir coin üretilir. Uzun kare/dolu nokta sonrası biriken
haklar aynı anda boşaltılmaz; yeni coin tam ömrüyle başlar. Süresi dolan
coinlerin skor için geçersiz kılınması veri modelinde yapılır.

## Zaman ve oturum davranışı

- Oyun Playing durumundayken ve kalibrasyon bitmişken zaman ilerler.
- Pause, uygulamanın arka plana geçmesi, kalibrasyon ve kazanma süreyi dondurur.
- Kalibrasyon bittikten sonra kalan süreler devam eder.
- Respawn, coin döngüsünü/skoru sıfırlamaz; mobil yeniden kalibrasyon süresince
  döngü bekler. Restart sahneyi yeniden kurar ve tüm oturum durumunu sıfırlar.
- Coinler bitse bile oyun sürer; doğrudan hedefe gidilebilir.
- Sonuçtaki 'toplanmayan toplam', süresi dolan + halen aktif + henüz doğmamış
  coinleri kapsar. HUD 'sırada' sayısı yalnızca henüz doğmamış hakları gösterir.
- Ayarları yayın sonrası değiştirirken karşılaştırılabilir skor için yeni
  bölüm kimliği veya açık bir kayıt göçü gerekir.

## Otomatik kontroller

Unity 2021.3.16f1 kütüphanelerine karşı C# derlemesi, mevcut M1–M7 veri testleri
ve asset bağlantı kontrolüne ek olarak `TimedCoinTests` eklendi:

- İlk gecikme, pause/kalibrasyon zamanı, tam süre dolumu, toplama/expiry tekilliği.
- Aktif limit, dolu konum ve güvenlik engelinde hakkın korunması.
- Aynı seed/restart sırası; farklı seed; güvenlik gecikmesinin sırayı bozmaması.
- Büyük karede birikmiş spawn patlamaması; yeni coinin tam ömrü.
- Hiç toplanmayan coinlerin sonunda tükenmesi, hepsinin toplanabildiği ideal
  veri akışında skor tavanı; geçersiz ID ve çift toplama engeli.
- Yanlış ayarlar, boş aday listesi ve gerçek GameConfig'teki yeni iki bölüm.
- Önceki altı bölümün static kalması, kimlikleri ve mevcut kayıtların devamlılığı.

Bu testlerde güvenli konum fonksiyonu taklit edilir; Unity Physics.CheckSphere,
trigger callback'leri, yanıp sönme, gerçek pause ve UI davranışı çalıştırılmaz.
Bellek içi kayıt testleri gerçek cihaz PlayerPrefs kalıcılığı anlamına gelmez.

```powershell
./Tools/Compile-Gameplay.ps1 M8
./Tools/Test-GameplayData.ps1
python Tools/Validate-Assets.py
```

## Manuel kabul listesi

1. MainMenu'yu aç, import/compile bitince Play'e bas. Önceki bölüm skorları
   korunmalı; 1–6. bölümlerde coinler eskisi gibi sabit kalmalı.
2. Bölüm 7'yi aç: coinler hemen doldurulmamalı. Kalibrasyon sonrası yaklaşık
   2 sn içinde ilk coin; uygun koşullarda 6 sn aralıklarla devamı gelmeli.
3. En fazla 3 aktif coin, toplam 10 hak olmalı. Bölüm 8'de değerler 4 ve 12.
4. Bir coin'i izle: son 5 saniyede yanıp sönmeli; ömrü bitince kaybolmalı.
   Kaybolan konumdan geçmek puan vermemeli; uyarı sırasında toplamak vermeli.
5. Coin topla: bir defa 100 puan. Aktif sayı azalmalı; toplam hak artmamalı.
6. İlk coin doğarken/dolmak üzereyken pause yap ve 30 sn bekle. Devam edince
   kalan süre korunmalı. Pause'da yeniden kalibrasyonu da dene.
7. Restart yap: skor, zamanlar ve toplam hak sıfırlanmalı; aynı spawn sırası
   gelmeli. Güvenlik nedeniyle konumda beklemek doğum saatini erteleyebilir.
8. Bir spawn noktasının üzerinde bekle: altında yeni coin doğmamalı. Uzaklaşınca
   sıradaki coin çıkmalı; sıradaki hak beklerken bütçe eksilmemeli.
9. Hiç coin toplamadan bekle: toplam hak sonunda tükenmeli; daha fazla coin
   çıkmamalı. HUD 'sırada 0/aktif 0/kaçan toplam hak' göstermeli. Hedef kazanmalı.
10. Tüm coinleri beklemeden hedefe ulaş: kazanma çalışmalı, sonuçtaki alınan ve
    toplanmayan sayıları toplam hakkı vermeli. Sonraki bölüm kilidi açılmalı.
11. Respawn yap: skor/coin bütçesi resetlenmemeli. Coinler harita duvarları,
    başlangıç veya hedefle çakışmamalı; dar bağlantılarda görünmemeli.
12. Android'de pause/arka plan/kalibrasyon, yanıp sönme görünürlüğü ve iki satırlı
    HUD'u farklı ekran oranlarında doğrula. Bu çalışmada cihaz testi yapılmadı.

Ömür/interval değerleri ilk ayarlardır; gerçek oynanışta bazı coinlere yetişme
zorluğu dengeleme isteyebilir. Güvenlik beklemesi sırasında spawn sayacı durmuş
gibi görünebilir; bu tasarlanan davranıştır, sınırsız yeni coin üretmez.
