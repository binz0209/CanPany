import { useI18n } from "../hooks/useI18n";
import Button from "./ui/button";

export default function LanguageSwitcher() {
  const { currentLanguage, setLanguage } = useI18n();

  const toggleLanguage = () => {
    const newLang = currentLanguage === "vi" ? "en" : "vi";
    setLanguage(newLang);
    // Components sẽ tự động re-render nhờ useI18n hook
  };

  return (
    <Button
      variant="outline"
      size="sm"
      onClick={toggleLanguage}
      className="min-w-[60px]"
    >
      {currentLanguage === "vi" ? "🇻🇳 VI" : "🇬🇧 EN"}
    </Button>
  );
}




