import { StyleSheet, Text, View } from "react-native";

type CabecalhoProps = {
    titulo: string,
    subtitulo?: string
}

export default function Cabecalho({
    titulo,
    subtitulo
}: CabecalhoProps) {

    return (
        <View style={styles.container}>
            <Text style={styles.titulo}>{ titulo }</Text>
            {
                subtitulo && (
                    <Text style={styles.subtitulo}>{ subtitulo }</Text>
                )
            }
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        alignItems: 'center',
        marginBottom: 20
    },

    titulo: {
        fontSize: 28,
        fontWeight: 'bold',
        textAlign: 'center',
    },

    subtitulo: {
        fontSize: 16,
        marginTop: 8,
        textAlign: 'center',
    },
});
