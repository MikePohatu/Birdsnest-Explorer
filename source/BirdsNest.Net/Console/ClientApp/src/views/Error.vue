<!-- Copyright (c) 2019-2023 "20Road"
20Road Limited [https://www.20road.com]

This file is part of Birdsnest Explorer.

Birdsnest Explorer is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see http://www.gnu.org/licenses/.
-->
<template>
    <div id="wrap" class="xy-center">
        <div id="code">{{ $t('word_Error') }} {{ $route.params.code }}</div>
        <div id="message" class="x-center">{{ $t('word_whoops') }}...</div>
    </div>
</template>

<style scoped>
#wrap {
    margin: auto;
    color:#606060;
}

#code {
    font-size: xx-large;
    font-weight: bold;
    margin: auto;
}

#message {
    font-size: large;
}
</style>