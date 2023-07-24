// Copyright (c) 2019-2023 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

export const DEFAULT_LOCALE = 'en';
import { LocaleMessages, VueMessageType, createI18n } from 'vue-i18n';

function loadLocaleMessages (): { [x: string]: LocaleMessages<VueMessageType>; } {
  const locales = import.meta.glob<Record<string, string>>('./locales/*.json', { eager: true });
  const messages: { [x: string]: LocaleMessages<VueMessageType>; } = {};

  for (const key in locales) {
    const matched = key.match(/([A-Za-z0-9-_]+)\./i);
    if (matched && matched.length > 1) {
      const locale = matched[1];
      messages[locale] = locales[key];
    }
  };
  return messages;
}

const i18n = createI18n({
  locale: import.meta.env.VUE_APP_I18N_LOCALE || DEFAULT_LOCALE,
  allowComposition: true,
  fallbackLocale: import.meta.env.VUE_APP_I18N_FALLBACK_LOCALE || DEFAULT_LOCALE,
  messages: loadLocaleMessages(),
  legacy: false, // you must set `false`, to use Composition API
  globalInjection: true
});

export default i18n;