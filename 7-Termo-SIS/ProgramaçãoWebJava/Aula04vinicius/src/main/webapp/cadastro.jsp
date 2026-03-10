<%@ page language="java" contentType="text/html; charset=UTF-8"
    pageEncoding="UTF-8"%>
<!DOCTYPE html>
<html>
<head>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <title>Cadastro de Produto</title>
</head>
	<body class="container mt-5">
	    <div class="card p-4 shadow">
	        <h2>Novo Produto</h2>
	        <form action="ProdutoServlet" method="post">
	            <div class="mb-3">
	                <label class="form-label">Nome:</label>
	                <input type="text" name="nome" class="form-control" required>
	            </div>
	            <div class="mb-3">
	                <label class="form-label">Preço:</label>
	                <input type="number" step="0.01" name="preco" class="form-control" required>
	            </div>
	            <button type="submit" class="btn btn-success w-100">Adicionar</button>
	        </form>
	    </div>
	</body>
</html>