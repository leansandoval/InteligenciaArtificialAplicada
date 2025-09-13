window.ContactSection = Vue.defineComponent({
    name: 'ContactSection',
    template: `
        <v-container class="py-10">
            <v-row justify="center">
                <v-col cols="12" md="8">
                    <v-card elevation="8" class="pa-6 mb-4" rounded="xl">
                        <v-card-title class="text-h4 font-weight-bold text-success text-center">
                            <v-icon size="40" color="success" class="mb-2">mdi-email</v-icon>
                            Contacto
                        </v-card-title>
                        <v-card-text class="text-center">
                            <span class="text-subtitle-1">Formulario y datos de contacto.</span><br>
                            <v-chip color="success" class="ma-2" label>Email: demo@correo.com</v-chip>
                            <v-chip color="info" class="ma-2" label>Tel: +54 11 1234-5678</v-chip>
                        </v-card-text>
                        <v-card-actions class="justify-center">
                            <v-btn color="success" size="large" class="mr-2">Enviar Mensaje</v-btn>
                            <v-btn color="info" size="large">Ver Perfil</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-col>
            </v-row>
        </v-container>
    `
});
