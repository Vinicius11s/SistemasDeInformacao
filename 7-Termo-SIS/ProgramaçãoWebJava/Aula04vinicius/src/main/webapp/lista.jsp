<%@ page language="java" contentType="text/html; charset=UTF-8" pageEncoding="UTF-8"%>
<%@ page import="java.util.List" %>
<%@ page import="model.Produto" %>
<!DOCTYPE html>
<html>
<head>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <title>Lista de Produtos</title>
</head>
<meta charset="UTF-8">
	<body class="container mt-5">
	    <div class="d-flex justify-content-between align-items-center mb-4">
	        <h1 class="text-primary">Produtos Cadastrados</h1>
	        <a href="cadastro.jsp" class="btn btn-outline-primary"> + Novo Cadastro</a>
	    </div>
	
	    <table class="table table-hover table-striped shadow-sm border">
	        <thead class="table-dark">
	            <tr>
	                <th>Código (ID)</th>
	                <th>Nome do Produto</th>
	                <th>Preço Unitário</th>
	            </tr>
	        </thead>
	        <tbody>
	            <% 
	                List<Produto> lista = (List<Produto>) request.getAttribute("listaProdutos");
	                if (lista != null) {
	                    for (Produto p : lista) { 
	            %>
	                <tr>
	                    <td class="text-muted"><%= p.getId() %></td>
	                    <td class="fw-bold"><%= p.getNome() %></td>
	                    <td class="text-success">R$ <%= String.format("%.2f", p.getPreco()) %></td>
	                </tr>
	            <% 
	                    } 
	                } 
	            %>
	        </tbody>
	    </table>
	</div>
	    <a href="cadastro.jsp" class="btn btn-primary">Voltar para Cadastro</a>
	</body>
</html>