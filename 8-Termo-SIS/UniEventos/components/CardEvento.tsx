import { Pressable, StyleSheet, Text, View } from "react-native";

type CardEventoProps = {
    id: number,
    titulo: string,
    descricao: string,
    local: string,
    data: string,
    status: string,
    onDelete: (id: number) => void,
    onDetalhe: (id: number) => void
}

export default function CardEvento({
    id,
    titulo,
    descricao,
    local,
    data,
    status,
    onDelete,
    onDetalhe
}: CardEventoProps) {

    return (
        <View style={styles.card}>
            <Text style={styles.titulo}>{ titulo }</Text>
            <Text style={styles.descricao}>{ descricao }</Text>
            <Text style={styles.info}>Local: { local }</Text>
            <Text style={styles.info}>Data: { data }</Text>

            <View style={styles.rodape}>
              <Text style={styles.status}>Status: { status }</Text>

              <Pressable
                  style={styles.botaoAcao}
                  onPress={() => onDetalhe(id)}
              >
                  <Text style={styles.textoAcao}>Detalhe</Text>
              </Pressable>

              <Pressable
                  style={styles.botaoAcao}
                  onPress={() => onDelete(id)}
              >
                  <Text style={styles.textoAcao}>Excluir</Text>
              </Pressable>
            </View>
        </View>
    );
}

const styles = StyleSheet.create({
    card: {
      borderWidth: 1,
      borderRadius: 8,
      padding: 16,
      marginBottom: 12,
    },

    titulo: {
      fontSize: 18,
      fontWeight: 'bold',
      marginBottom: 6,
    },

    descricao: {
      fontSize: 15,
      marginBottom: 8,
    },

    info: {
      fontSize: 14,
      marginBottom: 4,
    },

    status: {
      fontSize: 14,
      fontWeight: 'bold',
    },

    rodape: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      alignItems: 'center',
      marginTop: 12
    },

    botaoAcao: {
      borderWidth: 1,
      borderRadius: 6,
      padding: 8,
      alignItems: 'center',
    },

    textoAcao: {
      fontWeight: 'bold',
    },
  });
