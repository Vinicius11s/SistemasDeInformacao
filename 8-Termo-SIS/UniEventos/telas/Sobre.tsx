import { StyleSheet, Text, View } from "react-native";
import { NativeStackScreenProps } from "@react-navigation/native-stack";
import Cabecalho from "../components/Cabecalho";
import Menu from "../components/Menu";
import { RootStackParamList } from "../navigation/types";

type Props = NativeStackScreenProps<
    RootStackParamList,
    "Sobre"
>;

export default function Sobre({ navigation }: Props) {

    function navegar(tela: string) {
        if (tela === "inicio") {
            navigation.navigate("Inicio");
        }

        if (tela === "eventos") {
            navigation.navigate("Eventos");
        }

        if (tela === "sobre") {
            navigation.navigate("Sobre");
        }
    }

    return (
        <View style={styles.pagina}>
            <View style={styles.container}>
                <Cabecalho
                    titulo="SOBRE"
                    subtitulo="Projeto Integrador - Checkpoint 02"
                />

                <View style={styles.card}>
                    <Text style={styles.tituloCard}>UniEventos</Text>
                    <Text style={styles.texto}>
                        App de eventos da faculdade.
                    </Text>
                </View>

                <View style={styles.card}>
                    <Text style={styles.tituloCard}>Disciplina</Text>
                    <Text style={styles.texto}>Programação Mobile</Text>
                    <Text style={styles.texto}>Toledo Prudente Centro Universitário</Text>
                    <Text style={styles.texto}>Prof. Lenon Fachiano Silva</Text>
                </View>

                <View style={styles.card}>
                    <Text style={styles.tituloCard}>Checkpoint 02</Text>
                    <Text style={styles.texto}>
                        A lista de eventos agora vem da API.
                    </Text>
                </View>
            </View>

            <Menu
                telaAtual="sobre"
                onNavegar={navegar}
            />
        </View>
    );
}

const styles = StyleSheet.create({
    pagina: {
        flex: 1,
    },

    container: {
        flex: 1,
        padding: 24,
        paddingTop: 50,
    },

    card: {
        borderWidth: 1,
        borderRadius: 8,
        padding: 16,
        marginBottom: 12,
    },

    tituloCard: {
        fontSize: 18,
        fontWeight: "bold",
        marginBottom: 8,
    },

    texto: {
        fontSize: 15,
        marginBottom: 4,
    },
});
