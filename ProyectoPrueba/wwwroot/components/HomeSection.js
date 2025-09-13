window.HomeSection = Vue.defineComponent({
    name: 'HomeSection',
    template: `
        <v-container class="py-10">
            <v-row justify="center">
                <v-col cols="12" md="8">
                    <v-alert type="info" border="start" elevation="2" class="mb-4">
                        ¡Bienvenido! Esta es una demo avanzada con Vue y Vuetify.
                    </v-alert>
                    <v-card elevation="12" class="pa-6 mb-4" rounded="xl">
                        <v-card-title class="text-h4 font-weight-bold text-primary text-center">
                            <v-avatar size="80" class="mb-3">
                                <img src="https://cdn-icons-png.flaticon.com/512/1055/1055687.png" alt="Hola Mundo" />
                            </v-avatar>
                            Bienvenido a la Home
                        </v-card-title>
                        <v-card-text class="text-center">
                            Esta es la sección principal de la aplicación.<br>
                            <v-chip color="primary" class="ma-2" label>IA Aplicada</v-chip>
                            <v-chip color="secondary" class="ma-2" label>Vue + Vuetify</v-chip>
                        </v-card-text>
                        <v-card-actions class="justify-center">
                            <v-btn color="primary" size="large" class="mr-2">Acción 1</v-btn>
                            <v-btn color="secondary" size="large">Acción 2</v-btn>
                        </v-card-actions>
                    </v-card>
                    <!-- Ejemplos interactivos de componentes -->
                    <v-row class="mt-6" justify="center">
                        <v-col cols="12" md="6">
                            <v-alert type="success" class="mb-4">Este es un ejemplo de <strong>v-alert</strong> con tipo éxito.</v-alert>
                            <v-card elevation="4" class="mb-4">
                                <v-card-title>Ejemplo de v-card</v-card-title>
                                <v-card-text>Las tarjetas permiten agrupar contenido visualmente.</v-card-text>
                            </v-card>
                            <div class="mb-4">
                                <v-chip color="info" class="ma-2" label>Chip informativo</v-chip>
                                <v-chip color="success" class="ma-2" label>Chip éxito</v-chip>
                            </div>
                            <div>
                                <v-btn color="primary" class="mr-2">Botón primario</v-btn>
                                <v-btn color="error">Botón error</v-btn>
                            </div>
                        </v-col>
                    </v-row>
                </v-col>
            </v-row>
        </v-container>
    `
});
