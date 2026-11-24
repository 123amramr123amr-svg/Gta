# نموذج أندرويد أصلي ثلاثي الأبعاد

هذا مشروع جديد منفصل عن نموذج HTML القديم، وليس WebView. اللعبة هنا بـ **C#** على Godot .NET (محرك Godot مكتوب بـ C++)، وفي `Native/driving.cpp` دالة قيادة C++ اختيارية تستدعيها C#؛ لو المكتبة غير موجودة يستخدم النموذج نفس الحساب بـ C#. سكربت GDScript صغير داخل `addons` مسؤول فقط عن تضمين مكتبة C++ عند التصدير، وليس منطق اللعبة.

**الحدود:** نموذج أولي صغير، به مدينة إجرائية نهارية، شخصية، سيارة واحدة، قيادة ولمس. لا يتضمن بعد مهمات أو حيوانات أو التصميمات التفصيلية الموجودة في نموذج الويب، وليس إعادة إنتاج واقعية أو كاملة لـ GTA 6. السيارة لا تتصادم مع المباني حالياً. لم يُبنَ APK أو يُختبر على جهاز في هذه البيئة.

## تشغيل في المحرر
1. ثبّت Godot **4.3 .NET** (نسخة C# تحديداً) و.NET SDK 8.
2. افتح `project.godot` واضغط Build ثم F6/F5.
3. الحركة: WASD أو الأسهم، E للركوب والنزول، Space للفرامل. في أندرويد: المس منطقة التحكم أسفل اليسار والأزرار أسفل اليمين.

## بناء APK
1. جهّز متطلبات تصدير مشاريع C# لأندرويد في [توثيق Godot الرسمي](https://docs.godotengine.org/en/4.3/tutorials/export/exporting_for_android.html) و[تصدير C#](https://docs.godotengine.org/en/4.3/tutorials/export/exporting_for_android.html#c-projects): JDK 17، Android SDK وNDK، .NET SDK والأحمال المطلوبة، وقوالب تصدير Godot الموافقة لإصدار المحرر. دعم C# على Android في Godot تجريبي؛ قد تختلف خطوات الإصدار حسب الإعداد.
2. اضبط مسارات Java وAndroid SDK في إعدادات محرر Godot، وثبّت قوالب التصدير. استورد المشروع وابنه بنجاح.
3. اختياري: ابنِ مكتبة C++ لـ arm64-v8a باستخدام NDK، ثم انسخها قبل التصدير:
   ```sh
   cmake -S Native -B Native/build-arm64 \
     -DCMAKE_TOOLCHAIN_FILE="$ANDROID_NDK_HOME/build/cmake/android.toolchain.cmake" \
     -DANDROID_ABI=arm64-v8a -DANDROID_PLATFORM=android-24
   cmake --build Native/build-arm64 --config Release
   mkdir -p native/bin/arm64-v8a
   cp Native/build-arm64/libdriving.so native/bin/arm64-v8a/
   ```
   فعّل إضافة `Native driving library` من Project Settings > Plugins؛ إضافة التصدير تضمّن المكتبة إن وُجدت. بدونها يعمل بديل C#.
4. من Project > Export اختر `Android arm64` وصدّر **APK للتجربة**. إعدادات توقيع debug تأتي من إعدادات المحرر؛ التوزيع يتطلب مفتاح release خاص بك. ثبّت APK على جهاز arm64 واختبره. لا تحفظ مفاتيح التوقيع في المستودع.

التصدير يحتاج اتصالاً لتنزيل الأدوات مسبقاً؛ اللعبة نفسها لا تعتمد على HTML أو تحميل مكتبات من الإنترنت أثناء اللعب.

## بناء تجريبي تلقائي
يحتوي المشروع على سير عمل GitHub Actions في `.github/workflows/android-apk.yml` لمحاولة بناء APK تجريبي لـ arm64 ورفعه كـ artifact اسمه `CoastalCity-Android-debug`. افتح تبويب Actions واختر أحدث تشغيل ناجح لتنزيله؛ تحميل الـartifact يتطلب تسجيل الدخول إلى GitHub. ملف debug للتجربة فقط، وليس إصدار متجر. لا يوجد ضمان بنجاح البناء قبل تشغيله والتحقق من النتيجة.
