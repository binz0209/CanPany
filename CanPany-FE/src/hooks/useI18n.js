import { useState, useEffect } from 'react';
import { i18n } from '../services/i18n.service';

/**
 * Hook để sử dụng i18n trong React components
 * Tự động re-render khi language thay đổi
 */
export function useI18n() {
  const [currentLang, setCurrentLang] = useState(i18n.getCurrentLanguage());

  useEffect(() => {
    const handleLanguageChange = (e) => {
      setCurrentLang(e.detail);
    };

    window.addEventListener('languageChanged', handleLanguageChange);
    return () => {
      window.removeEventListener('languageChanged', handleLanguageChange);
    };
  }, []);

  return {
    t: (key, params) => i18n.t(key, params),
    currentLanguage: currentLang,
    setLanguage: (lang) => {
      i18n.setLanguage(lang);
      setCurrentLang(lang);
    },
  };
}

export default useI18n;

