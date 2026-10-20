import { Pressable, StyleSheet, Text } from "react-native";

type BotaoProps = {
  titulo: string,
  onPress: () => void
}

export default function Botao({
  titulo,
  onPress
}: BotaoProps) {

    return (
        <Pressable
          style={styles.botao}
          onPress={ onPress }
        >
          <Text style={styles.texto} >
            { titulo }
          </Text>
        </Pressable>
    );
}

const styles = StyleSheet.create({
  botao: {
     borderWidth: 1,
     borderRadius: 8,
     padding: 12,
     alignItems: 'center',
     marginTop: 10,
   },

    texto: {
     fontSize: 16,
     fontWeight: 'bold',
   },
 });
