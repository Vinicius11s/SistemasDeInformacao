import { useEffect, useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import { NativeStackScreenProps } from "@react-navigation/native-stack";
import Cabecalho from "../components/Cabecalho";
import Botao from "../components/Botao";
import Menu from "../components/Menu";
import { RootStackParamList } from "../navigation/types";
import { listar } from "../services/eventos";

type Props = NativeStackScreenProps<
    RootStackParamList,
    "Inicio"
>;

export default function Inicio({ navigation }: Props) {

    const [totalEventos, setTotalEventos] = useState(0);
    const [carregando, setCarregando] = useState(true);
    const [erroApi, setErroApi] = useState("");

    useEffect(() => {
        const unsubscribe = navigation.addListener("focus", () => {
            carregar();
        });

        return unsubscribe;
    }, [navigation]);

    async function carregar() {
        try {
            setCarregando(true);
            setErroApi("");
            const lista = await listar();
            setTotalEventos(lista.length);
        } catch (e) {
            setTotalEventos(0);
            if (e instanceof Error && e.message === "servidor") {
                setErroApi("Erro enviado pelo servidor.");
            } else {
                setErroApi("Falha na comunicação com o servidor.");
            }
        } finally {
            setCarregando(false);
        }
    }

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
                    titulo="UNIEVENTOS"
                    subtitulo="Agenda universitária"
                />

                <View style={styles.card}>
                    <Text style={styles.tituloCard}>Início</Text>
                    <Text style={styles.texto}>
                        App para ver e cadastrar eventos da faculdade.
                    </Text>
                    <Text style={styles.destaque}>
                        {
                            carregando ?
                            "Carregando..." :
                            erroApi !== "" ?
                            erroApi :
                            totalEventos > 0 ?
                            `${totalEventos} eventos cadastrados` :
                            "Nenhum evento cadastrado"
                        }
                    </Text>
                </View>

                <View style={styles.card}>
                    <Text style={styles.tituloCard}>Telas</Text>
                    <Text style={styles.texto}>Início</Text>
                    <Text style={styles.texto}>Eventos</Text>
                    <Text style={styles.texto}>Detalhe</Text>
                    <Text style={styles.texto}>Sobre</Text>
                </View>

                <Botao
                    titulo="Ver eventos"
                    onPress={() => navigation.navigate("Eventos")}
                />
            </View>

            <Menu
                telaAtual="inicio"
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

    destaque: {
        fontSize: 16,
        fontWeight: "bold",
        marginTop: 10,
    },
});
