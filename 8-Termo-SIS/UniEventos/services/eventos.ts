import { Evento } from "../models/Evento";

const urlBase = "http://192.168.24.26:5141/api/eventos";

export async function listar() {
    const response = await fetch(urlBase, { cache: "no-store" });

    if (!response.ok) {
        throw new Error("servidor");
    }

    const dados: Evento[] = await response.json();
    return dados;
}

export async function buscarPorId(id: number) {
    const response = await fetch(urlBase + "/" + id, { cache: "no-store" });

    if (!response.ok) {
        throw new Error("servidor");
    }

    const dados: Evento = await response.json();
    return dados;
}

export async function cadastrar(evento: {
    titulo: string,
    descricao: string,
    local: string,
    data: string
}) {
    const response = await fetch(urlBase, {
        method: "POST",
        cache: "no-store",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(evento)
    });

    if (!response.ok) {
        throw new Error("servidor");
    }

    const dados: Evento = await response.json();
    return dados;
}

export async function excluir(id: number) {
    const response = await fetch(urlBase + "/" + id, {
        method: "DELETE",
        cache: "no-store"
    });

    if (!response.ok) {
        throw new Error("servidor");
    }
}
