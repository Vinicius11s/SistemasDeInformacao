import { useEffect, useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import { NativeStackScreenProps } from "@react-navigation/native-stack";
import { Evento } from "../models/Evento";
import Cabecalho from "../components/Cabecalho";
import Botao from "../components/Botao";
import { RootStackParamList } from "../navigation/types";
import { buscarPorId } from "../services/eventos";

type Props = NativeStackScreenProps<
    RootStackParamList,
    "Detalhe"
>;

export default function Detalhe({ route, navigation }: Props) {

    const [evento, setEvento] = useState<Evento | null>(null);
    const [carregando, setCarregando] = useState(true);
    const [erroApi, setErroApi] = useState("");

    useEffect(() => {
        async function buscar() {
            try {
                setCarregando(true);
                setErroApi("");
                const dados = await buscarPorId(route.params.id);
                setEvento(dados);
            } catch (e) {
                setEvento(null);
                if (e instanceof Error && e.message === "servidor") {
                    setErroApi("Erro enviado pelo servidor.");
                } else {
                    setErroApi("Falha na comunicação com o servidor.");
                }
            } finally {
                setCarregando(false);
            }
        }

        buscar();
    }, [route.params.id]);

    if (carregando) {
        return (
            <View style={styles.container}>
                <Cabecalho
                    titulo="DETALHE"
                    subtitulo="Informações do evento"
                />
                <Text style={styles.texto}>Carregando...</Text>
                <Botao
                    titulo="Voltar para eventos"
                    onPress={() => navigation.goBack()}
                />
            </View>
        );
    }

    if (erroApi !== "" || !evento) {
        return (
            <View style={styles.container}>
                <Cabecalho
                    titulo="DETALHE"
                    subtitulo="Informações do evento"
                />
                <Text style={styles.texto}>
                    { erroApi !== "" ? erroApi : "Erro enviado pelo servidor." }
                </Text>
                <Botao
                    titulo="Voltar para eventos"
                    onPress={() => navigation.goBack()}
                />
            </View>
        );
    }

    return (
        <View style={styles.container}>
            <Cabecalho
                titulo="DETALHE"
                subtitulo="Informações do evento"
            />

            <View style={styles.card}>
                <Text style={styles.titulo}>{ evento.titulo }</Text>
                <Text style={styles.texto}>{ evento.descricao }</Text>
                <Text style={styles.info}>Local: { evento.local }</Text>
                <Text style={styles.info}>Data: { evento.data }</Text>
                <Text style={styles.status}>Status: { evento.status }</Text>
            </View>

            <View style={styles.card}>
                <Text style={styles.tituloCard}>Em desenvolvimento</Text>
                <Text style={styles.texto}>
                    Edição e inscrição ficam para os próximos checkpoints.
                </Text>
            </View>

            <Botao
                titulo="Voltar para eventos"
                onPress={() => navigation.goBack()}
            />
        </View>
    );
}

const styles = StyleSheet.create({
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

    titulo: {
        fontSize: 20,
        fontWeight: "bold",
        marginBottom: 8,
    },

    tituloCard: {
        fontSize: 18,
        fontWeight: "bold",
        marginBottom: 8,
    },

    texto: {
        fontSize: 15,
        marginBottom: 8,
    },

    info: {
        fontSize: 15,
        marginBottom: 4,
    },

    status: {
        fontSize: 15,
        fontWeight: "bold",
        marginTop: 8,
    },
});
