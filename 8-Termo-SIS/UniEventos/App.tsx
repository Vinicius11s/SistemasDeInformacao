import { StyleSheet, useWindowDimensions, View } from "react-native";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { NavigationContainer } from "@react-navigation/native";
import Inicio from "./telas/Inicio";
import Eventos from "./telas/Eventos";
import Detalhe from "./telas/Detalhe";
import Sobre from "./telas/Sobre";
import { RootStackParamList } from "./navigation/types";

const Stack = createNativeStackNavigator<RootStackParamList>();

export default function App() {

    const { width } = useWindowDimensions();
    const telaGrande = width >= 768;

    return (
        <View style={styles.container}>
            <View style={[
                styles.conteudo,
                telaGrande && styles.conteudoGrande
            ]}>
                <NavigationContainer>
                    <Stack.Navigator
                        id="MainStack"
                        screenOptions={{ headerShown: false }}
                    >
                        <Stack.Screen
                            name="Inicio"
                            component={Inicio}
                        />

                        <Stack.Screen
                            name="Eventos"
                            component={Eventos}
                        />

                        <Stack.Screen
                            name="Detalhe"
                            component={Detalhe}
                        />

                        <Stack.Screen
                            name="Sobre"
                            component={Sobre}
                        />
                    </Stack.Navigator>
                </NavigationContainer>
            </View>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
    },

    conteudo: {
        flex: 1,
    },

    conteudoGrande: {
        alignSelf: "center",
        width: "80%",
    }
});
