# GTA VI — Leonida Fan Game (Android)

لعبة Fan-Made مستوحاة من GTA 6 — Vice City / Leonida. مشروع تعليمي، مش لعبة Rockstar الرسمية.

## تشغيل اللعبة دلوقتي
افتح `game/index.html` في متصفح الموبايل أو الكمبيوتر — شغالة مباشرة (Three.js WebGL).

- **موبايل:** جويستيك شمال للحركة، أزرار يمين (اركب / بدّل / اجري / راديو)
- **PC:** WASD حركة، Shift جري، E ركوب عربية، Tab تبديل Jason/Lucia، Q كاميرا

## اللي موجود في المرحلة 1
- عالم Vice City 3D: مباني نيون، نخيل، شاطئ، بحر، غروب
- Jason Duval + Lucia Caminos (تبديل فوري)
- 8 عربيات قابلة للقيادة + مرور
- نظام Wanted (نجوم) + شرطة بتطارد
- مهمة "Easy Score": خد الشنطة واهرب
- راديو Vice City FM (موسيقى Synthwave أصلية)
- خريطة مصغرة + فلوس + صحة

## الوثائق
- `docs/GTA6_RESEARCH.md` — بحث كامل: قصة، شخصيات، موسيقى، خريطة، عربيات
- `docs/GAME_PLAN.md` — خطة المراحل
- `docs/assets/` — صور 3D للشخصيات و Vice City

## تحويلها لـ APK أندرويد
1. ثبّت Node.js
2. `npm i @capacitor/core @capacitor/cli @capacitor/android`
3. انسخ `game/` في `www/` واعمل `npx cap init` + `npx cap add android`
4. افتح Android Studio واعمل Build APK

## المراحل الجاية
المرحلة 2: فيزياء قيادة أفضل + درفت | المرحلة 3: قتال وأسلحة | المرحلة 4: مهمات أكتر | المرحلة 5: Keys و Grassrivers
