import { Pressable, StyleSheet, Text, View } from "react-native";

type MenuProps = {
    telaAtual: string,
    onNavegar: (tela: string) => void
}

export default function Menu({
    telaAtual,
    onNavegar
}: MenuProps) {

    return (
        <View style={styles.container}>
            <Pressable
                style={[
                    styles.item,
                    telaAtual === 'inicio' && styles.itemAtivo
                ]}
                onPress={() => onNavegar('inicio')}
            >
                <Text style={styles.texto}>Início</Text>
            </Pressable>

            <Pressable
                style={[
                    styles.item,
                    telaAtual === 'eventos' && styles.itemAtivo
                ]}
                onPress={() => onNavegar('eventos')}
            >
                <Text style={styles.texto}>Eventos</Text>
            </Pressable>

            <Pressable
                style={[
                    styles.item,
                    telaAtual === 'sobre' && styles.itemAtivo
                ]}
                onPress={() => onNavegar('sobre')}
            >
                <Text style={styles.texto}>Sobre</Text>
            </Pressable>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        gap: 8,
        padding: 12,
        borderTopWidth: 1,
    },

    item: {
        flex: 1,
        borderWidth: 1,
        borderRadius: 8,
        padding: 10,
        alignItems: 'center',
    },

    itemAtivo: {
        backgroundColor: '#e6e6e6',
    },

    texto: {
        fontWeight: 'bold',
    },
});
