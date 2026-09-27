# Mobil Labirent - Gelistirme Plani

## Onaylanan kapsam
Kucuk, yayinlanabilir bir 3D mobil labirent oyunu. Ana amac hedefe ulasmak;
coinler istege bagli skor saglar. Mobil tilt ve Editor klavyesi ayni motoru
kullanir. Placeholder gorseller dis asset gerektirmez.

Nihai akis: Ana Menu -> Bolum -> Coin/Hedef -> Sonuc -> Sonraki/Tekrar.
Pause, restart, guvenli respawn, bolum/coin HUD ve acilan bolum kaydi gerekir.

## Mimari
MainMenu ve Gameplay sahneleri. GameSession yalnizca oyun durumunu koordine
eder. LevelDefinition/LevelManager bolum verisi ve kurulumunu; MazeGenerator,
MazeValidator ve MazeBuilder veri uretimi, erisilebilirlik ve geometriyi ayirir.
PlayerInput, PlayerMotor, PlayerVisual ve PlayerRespawn ayri sorumluluklardir.
Coin/CoinSpawner, ScoreService, ProgressionService/SaveService ve UI ayri kalir.
Tek Player kokunde Rigidbody/collider; alt Visual nesnesinde model/Animator.
Ziplama ilk surum kapsaminda degildir.

Once sabit hucre verisi, sonra deterministik seed ile baglantili labirent.
Yayin bolumleri secilmis/test edilmis seed kullanir. Hedef ve coinler
erisilebilir hucrelere yerlestirilir. Hareketli duvarlar daha sonra guvenli
koridorlarda hareket eder; oyuncu hareket alanindaysa bekler ve gecis
boyunca hedefe alternatif yol korunur.

## Milestone ve kabul kosullari
1. Temizlik: tek Player, tutarli tag/collider; eski sahnelerde hareket,
   coin ve hedef temasi. Tamamlandi; kullanici Play Mode testini yapti ve sorun bildirmedi.
2. Player Root/Visual ve kamera: model degisimi fizigi bozmamali.
3. Ortak klavye/tilt motoru: kalibrasyon, dead zone, hassasiyet, filtre,
   maksimum hiz, ekran yonu; gercek cihaz testi.
4. Sabit labirentte tam menu/oyun/sonuc akisi; pause/restart/respawn.
5. Coin/skor: HUD ve sonuc; cift toplama engeli.
6. Kayit: acilan bolum, en iyi skor, ayarlar; tekrar odulu tutarliligi.
7. Uretim: farkli boyut, seed, hedef erisimi ve gecerli yerlestirme testleri.
8. Rastgele/sureli coin: guvenli spawn, adet/sure, pause davranisi.
9. Hareketli duvar: oyuncuyu sikistirmayan, yolu kapatmayan hareket.
10. UI/ses/gorsel polish, performans, Android build ve cihaz kabul testleri.
    Magaza yayin gereksinimleri ayrica dogrulanir.

M2-M6 sirasiyla uygulandi ve kullanici kabul testlerini onayladi:
[Milestones2-6.md](Milestones2-6.md).
M7 uygulama ve manuel kabul raporu: [Milestone7.md](Milestone7.md).
M8 uygulama ve manuel kabul raporu: [Milestone8.md](Milestone8.md).
M9 baslatilmadi.

## Milestone 1 - yapilanlar
- Assets/prefabs/Player.prefab tek Player olarak korundu; GUID degismedi.
- Player etiketi duzeltildi; kapali BoxCollider kaldirildi, CapsuleCollider
  korundu. Rotation kilidi ve gravity korundu. Interpolation ve continuous
  collision detection acildi.
- level0/level1 collider ve hiz override'lari kaldirildi; ortak MoveSpeed 1000.
- Rigidbody Awake'te ayni nesneden alinir, RequireComponent zorunlulugu var.
  Kullanilmayan public Rigidbody, Coin, JumpSpeed, IsGrounded alanlari ve
  ziplama kaldirildi. Diyagonal giris normalize edildi. Mevcut kuvvet
  olceklemesi milestone 3'e kadar korundu.
- Coin, attachedRigidbody uzerindeki playerScript ile oyuncuyu tanir;
  birden fazla collider temasi ayni coin'i tekrar isleyemez.
- Kamera bos hedefte kendini kapatir; takip LateUpdate'tedir.
- level0 kapali Sphere ve zemindeki fazladan BoxCollider kaldirildi.
- level1 zemin etiketi Ground yapildi.
- Referanssiz Assets/Player.prefab, bos level2 ve bozuk/referanssiz
  Assets/Materials/indir.mat meta dosyalariyla kaldirildi.
- Bos Pet Your Cat (Demo) klasoru/meta kaldirildi.
- Unity/IDE uretim dosyalari icin .gitignore eklendi.
- Mevcut iki labirent, Aim, kapali followCamera, DOTween ve Android
  sablonlari bu milestone'da korundu.

## Dogrulama
- Uc oyun scripti Unity 2021.3.16f1'in kendi Mono C# derleyicisi ve
  CoreModule/PhysicsModule/InputLegacyModule kutuphaneleriyle hatasiz derlendi.
  Cikti: Temp/Milestone1/Gameplay.dll (git tarafindan ignore edilir).
- Tum sahne/prefab yerel fileID baglantilari dogrulandi.
- Sahne/prefab/materyal GUID baglantilarinin tumu cozuldu.
- Silinen assetlerin GUID referanslari silmeden once kontrol edildi.
- git diff --check gecti.
- Tam Unity import/build ve Play Mode testi yapilmadi. Acik Unity oturumu
  kapatilmadi; ikinci Editor baslatilmadi.

## M1 Play Mode kabul testi (kullanici tarafindan onaylandi)
1. Editor asset yenilemesini tamamlasin; Console'da compile/import hatasi olmasin.
2. level0 acilsin: WASD/oklar hareket ettirsin; Space ziplatmasin.
3. Duvarlar oyuncuyu tutsun; oyuncu zeminden gecmesin.
4. Coin'e temas nesneyi kaldirsin; Aim temasi Goal reached mesajini uretsin.
5. level1'de ayni fizik ayarlariyla hareket ve hedef temasi dogrulansin;
   ozellikle dar koridorlarin ortak collider ile gecilebildigi kontrol edilsin.
6. Takip kamerasi hedef atanmadan acilirsa tek uyari verip kapansin;
   null referans hatasi olmasin.

Bu bolum M1 tarihcesidir. Yeni oyun akisinin durumu Milestones2-6.md
dosyasindadir. Yayin kabul testleri henuz yapilmamistir.
