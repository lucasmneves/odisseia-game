// Ponte JS <-> C# usada só por MobilePlatformDetector e SafeAreaController.
// Não usa nenhuma API específica de Android/iOS — só APIs padrão de navegador
// (navigator.userAgent, matchMedia, touch events, CSS env()), que existem tanto no
// Safari iOS quanto no Chrome Android.

mergeInto(LibraryManager.library, {

  // Combina user agent + capacidade de toque. Nenhum dos dois sozinho é confiável:
  // UA pode ser falsificado/omitido, e notebooks com tela touch têm maxTouchPoints > 0
  // sem serem celulares. Exigir os dois reduz falso positivo/negativo.
  Odisseia_IsMobileBrowser: function () {
    try {
      var ua = navigator.userAgent || navigator.vendor || window.opera || "";
      var uaLooksMobile = /Android|iPhone|iPad|iPod|IEMobile|BlackBerry|Opera Mini|Mobile|webOS/i.test(ua);

      // iPadOS 13+ se identifica como "Macintosh" no user agent, mas expõe múltiplos
      // pontos de toque (um Mac de verdade não tem touch multi-ponto).
      var isIpadOS = navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1;

      var hasTouch = ("ontouchstart" in window) || (navigator.maxTouchPoints > 0);
      var coarsePointer = !!(window.matchMedia && window.matchMedia("(pointer: coarse)").matches);

      return ((uaLooksMobile || isIpadOS) && (hasTouch || coarsePointer)) ? 1 : 0;
    } catch (e) {
      return 0;
    }
  },

  // O template WebGL padrão do Unity não inclui "viewport-fit=cover" na meta viewport,
  // então env(safe-area-inset-*) sempre resolve para 0 sem isto — mesmo num iPhone com
  // notch. Complementa a meta tag em vez de substituí-la.
  Odisseia_PatchViewportForSafeArea: function () {
    try {
      var meta = document.querySelector('meta[name="viewport"]');
      if (meta && meta.content.indexOf('viewport-fit=cover') === -1) {
        meta.content = meta.content + ', viewport-fit=cover';
      }
    } catch (e) {
      // Sem meta viewport (não deveria acontecer no template padrão do Unity): os
      // insets ficam 0, que é o mesmo que "sem safe area" — degrada sem quebrar.
    }
  },

  // Lê os insets de safe area via um elemento de sonda com env(safe-area-inset-*) no
  // CSS, e devolve "top,right,bottom,left" em px CSS. É 0 em qualquer navegador/
  // dispositivo sem notch — não é uma API Android/iOS, é CSS padrão (iOS Safari e
  // Chrome Android tratam igual).
  Odisseia_GetSafeAreaInsets: function () {
    try {
      var probe = document.getElementById('odisseia-safe-area-probe');
      if (!probe) {
        probe = document.createElement('div');
        probe.id = 'odisseia-safe-area-probe';
        probe.style.position = 'fixed';
        probe.style.top = 'env(safe-area-inset-top, 0px)';
        probe.style.right = 'env(safe-area-inset-right, 0px)';
        probe.style.bottom = 'env(safe-area-inset-bottom, 0px)';
        probe.style.left = 'env(safe-area-inset-left, 0px)';
        probe.style.visibility = 'hidden';
        probe.style.pointerEvents = 'none';
        document.body.appendChild(probe);
      }

      var cs = getComputedStyle(probe);
      var result = [cs.top, cs.right, cs.bottom, cs.left].join(',');

      var bytes = lengthBytesUTF8(result) + 1;
      var buffer = _malloc(bytes);
      stringToUTF8(result, buffer, bytes);
      return buffer;
    } catch (e) {
      return 0;
    }
  }

});
