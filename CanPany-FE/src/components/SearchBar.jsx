import Input from './ui/input'
import Button from './ui/button'
import { useI18n } from '../hooks/useI18n'

export default function SearchBar() {
  const { t } = useI18n();
    return (
        <div className="flex gap-2">
            <Input placeholder={t("Search.Placeholder")} />
            <Button className="whitespace-nowrap">{t("Common.Filter")}</Button>
        </div>
    )
}