window.AboutSection = Vue.defineComponent({
    name: 'AboutSection',
    template: `
        <v-container class="py-10">
            <v-row justify="center">
                <v-col cols="12" md="8">
                    <v-card elevation="8" class="pa-6 mb-4" rounded="xl">
                        <v-card-title class="text-h4 font-weight-bold text-secondary text-center">
                            <v-icon size="40" color="secondary" class="mb-2">mdi-information</v-icon>
                            Sobre Nosotros
                        </v-card-title>
                        <v-card-text class="text-center">
                            <span class="text-subtitle-1">Información sobre el proyecto y el equipo.</span><br>
                            <v-chip color="primary" class="ma-2" label>ASP.NET Core</v-chip>
                            <v-chip color="secondary" class="ma-2" label>Vue.js</v-chip>
                            <v-chip color="success" class="ma-2" label>Vuetify</v-chip>
                        </v-card-text>
                    </v-card>
                </v-col>
            </v-row>
        </v-container>
    `
});
