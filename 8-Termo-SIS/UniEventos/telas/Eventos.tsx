import { useEffect, useState } from "react";
import { FlatList, KeyboardAvoidingView, Platform, StyleSheet, Text, TextInput, View } from "react-native";
import { NativeStackScreenProps } from "@react-navigation/native-stack";
import { Evento } from "../models/Evento";
import Cabecalho from "../components/Cabecalho";
import Botao from "../components/Botao";
import CardEvento from "../components/CardEvento";
import Menu from "../components/Menu";
import { RootStackParamList } from "../navigation/types";
import { cadastrar as cadastrarEvento, excluir as excluirEvento, listar } from "../services/eventos";

type Props = NativeStackScreenProps<
    RootStackParamList,
    "Eventos"
>;

export default function Eventos({ navigation }: Props) {

    const [eventos, setEventos] = useState<Evento[]>([]);
    const [titulo, setTitulo] = useState("");
    const [descricao, setDescricao] = useState("");
    const [local, setLocal] = useState("");
    const [data, setData] = useState("");
    const [erro, setErro] = useState("");
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
            setEventos(lista);
        } catch (e) {
            setEventos([]);
            if (e instanceof Error && e.message === "servidor") {
                setErroApi("Erro enviado pelo servidor.");
            } else {
                setErroApi("Falha na comunicação com o servidor.");
            }
        } finally {
            setCarregando(false);
        }
    }

    async function cadastrar() {
        if (
            titulo.trim() === "" ||
            descricao.trim() === "" ||
            local.trim() === "" ||
            data.trim() === ""
        ) {
            setErro("Título, descrição, local e data são obrigatórios!");
            return;
        }

        try {
            setErro("");
            setErroApi("");
            await cadastrarEvento({
                titulo: titulo,
                descricao: descricao,
                local: local,
                data: data
            });

            setTitulo("");
            setDescricao("");
            setLocal("");
            setData("");
            await carregar();
        } catch (e) {
            if (e instanceof Error && e.message === "servidor") {
                setErroApi("Erro enviado pelo servidor.");
            } else {
                setErroApi("Falha na comunicação com o servidor.");
            }
        }
    }

    async function excluir(id: number) {
        try {
            setErroApi("");
            await excluirEvento(id);
            await carregar();
        } catch (e) {
            if (e instanceof Error && e.message === "servidor") {
                setErroApi("Erro enviado pelo servidor.");
            } else {
                setErroApi("Falha na comunicação com o servidor.");
            }
        }
    }

    function abrirDetalhe(id: number) {
        navigation.navigate("Detalhe", { id: id });
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
        <KeyboardAvoidingView
            style={styles.pagina}
            behavior={Platform.OS === "ios" ? "padding" : undefined}
        >
            <FlatList
                style={styles.lista}
                data={carregando ? [] : eventos}
                keyExtractor={(item) => item.id.toString()}
                keyboardShouldPersistTaps="handled"
                contentContainerStyle={styles.conteudo}
                ListHeaderComponent={
                    <View>
                        <Cabecalho
                            titulo="EVENTOS"
                            subtitulo="Cadastro e lista de eventos"
                        />

                        <Text style={styles.tituloFormulario}>Novo Evento</Text>

                        <Text style={styles.label}>Título</Text>
                        <TextInput
                            style={styles.input}
                            placeholder="Digite o título"
                            value={titulo}
                            onChangeText={setTitulo}
                        />

                        <Text style={styles.label}>Descrição</Text>
                        <TextInput
                            style={styles.input}
                            placeholder="Digite a descrição"
                            value={descricao}
                            onChangeText={setDescricao}
                        />

                        <Text style={styles.label}>Local</Text>
                        <TextInput
                            style={styles.input}
                            placeholder="Digite o local"
                            value={local}
                            onChangeText={setLocal}
                        />

                        <Text style={styles.label}>Data</Text>
                        <TextInput
                            style={styles.input}
                            placeholder="Ex: 25/09/2026"
                            value={data}
                            onChangeText={setData}
                        />

                        {
                            erro !== "" && (
                                <Text style={styles.erro}>{ erro }</Text>
                            )
                        }

                        <Botao
                            titulo="Adicionar"
                            onPress={cadastrar}
                        />

                        <View style={styles.cabecalhoLista}>
                            <Text style={styles.tituloLista}>LISTA</Text>
                            <Text style={styles.tituloLista}>
                                {
                                    carregando ?
                                    "Carregando..." :
                                    erroApi !== "" ?
                                    "Erro na consulta" :
                                    eventos.length > 0 ?
                                    `${eventos.length} eventos` :
                                    "Nenhum evento"
                                }
                            </Text>
                        </View>

                        {
                            erroApi !== "" && (
                                <Text style={styles.erro}>{ erroApi }</Text>
                            )
                        }
                    </View>
                }
                renderItem={({item}) => (
                    <CardEvento
                        id={item.id}
                        titulo={item.titulo}
                        descricao={item.descricao}
                        local={item.local}
                        data={item.data}
                        status={item.status}
                        onDelete={excluir}
                        onDetalhe={abrirDetalhe}
                    />
                )}
                ListEmptyComponent={
                    !carregando && erroApi === "" ? (
                        <View style={styles.listaVazia}>
                            <Text style={styles.textoListaVazia}>
                                Nenhum evento cadastrado
                            </Text>
                            <Text>
                                Utilize o formulário para adicionar um evento.
                            </Text>
                        </View>
                    ) : null
                }
            />

            <Menu
                telaAtual="eventos"
                onNavegar={navegar}
            />
        </KeyboardAvoidingView>
    );
}

const styles = StyleSheet.create({
    pagina: {
        flex: 1,
    },

    lista: {
        flex: 1,
    },

    conteudo: {
        padding: 24,
        paddingTop: 50,
    },

    tituloFormulario: {
        fontSize: 22,
        fontWeight: "bold",
        marginBottom: 15,
    },

    label: {
        fontSize: 15,
        fontWeight: "bold",
        marginTop: 10,
        marginBottom: 5,
    },

    input: {
        width: "100%",
        borderWidth: 1,
        borderRadius: 8,
        padding: 12,
        fontSize: 16,
    },

    erro: {
        marginTop: 10,
        fontWeight: "bold",
    },

    tituloLista: {
        fontSize: 18,
        fontWeight: "bold",
        marginTop: 35,
        marginBottom: 15,
    },

    cabecalhoLista: {
        alignSelf: "stretch",
        justifyContent: "space-between",
        flexDirection: "row",
    },

    listaVazia: {
        borderWidth: 1,
        borderRadius: 8,
        padding: 20,
        alignItems: "center",
    },

    textoListaVazia: {
        fontSize: 17,
        fontWeight: "bold",
        marginBottom: 5,
    },
});
