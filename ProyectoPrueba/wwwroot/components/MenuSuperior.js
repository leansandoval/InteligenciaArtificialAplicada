window.MenuSuperior = Vue.defineComponent({
    name: 'MenuSuperior',
    template: `
        <v-app-bar color="primary" dark flat>
            <v-toolbar-title>
                <v-icon class="mr-2">mdi-home</v-icon>
                Proyecto IA Aplicada
            </v-toolbar-title>
            <v-spacer></v-spacer>
            <v-btn icon @click="$emit('cambiar-seccion', 'home')" :title="'Ir a Home'">
                <v-icon>mdi-home</v-icon>
            </v-btn>
            <v-btn icon @click="$emit('cambiar-seccion', 'about')" :title="'Sobre Nosotros'">
                <v-icon>mdi-information</v-icon>
            </v-btn>
            <v-btn icon @click="$emit('cambiar-seccion', 'contact')" :title="'Contacto'">
                <v-icon>mdi-email</v-icon>
            </v-btn>
            <v-divider vertical class="mx-2"></v-divider>
            <v-avatar size="32" class="mr-2">
                <img src="https://randomuser.me/api/portraits/men/1.jpg" alt="Usuario" />
            </v-avatar>
            <span class="font-weight-medium">Usuario</span>
        </v-app-bar>
    `,
    props: ['seccion']
});
