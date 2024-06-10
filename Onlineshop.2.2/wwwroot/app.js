// Функция для отправки GET запроса на сервер
async function fetchShopList() {
    const response = await fetch('/');
    const data = await response.json();
    // Обработка полученных данных
    console.log(data);
}

// Функция для отправки POST запроса на сервер для добавления магазина
async function addShop() {
    const shopName = document.getElementById('shopName').value;
    const shopAddress = document.getElementById('shopAddress').value;
    const response = await fetch('/adshop', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            Name: shopName,
            Address: shopAddress
        })
    });
    // Обработка ответа от сервера
    if (response.status === 201) {
        console.log('Shop added successfully');
    } else {
        console.error('Failed to add shop');
    }
}

// Функция для отправки POST запроса на сервер для добавления товара
async function addProduct() {
    const productName = document.getElementById('productName').value;
    const productPrice = document.getElementById('productPrice').value;
    const productCount = document.getElementById('productCount').value;
    const productDescription = document.getElementById('productDescription').value;
    const response = await fetch('/adshop/', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            Name: productName,
            Price: productPrice,
            Count: productCount,
            Description: productDescription
        })
    });
    // Обработка ответа от сервера
    if (response.status === 201) {
        console.log('Product added successfully');
    } else {
        console.error('Failed to add product');
    }
}


// Обновляем функцию loadShops для загрузки списка магазинов в выпадающий список удаления
async function loadShopsForDeletion() {
    try {
        const response = await fetch('/shop');
        const shops = await response.json();
        const deleteShopSelect = document.getElementById('shop-select');
        deleteShopSelect.innerHTML = '';
        shops.forEach(shop => {
            const option = document.createElement('option');
            option.value = shop.id;
            option.textContent = shop.name;
            deleteShopSelect.appendChild(option);
        });
    } catch (error) {
        console.error('Error loading shops:', error);
    }
}

// Обновляем обработчик события загрузки страницы
window.onload = function () {
    loadShops();
    loadShopsForDeletion(); // Загружаем список магазинов для удаления
    // Привязываем событие submit формы создания магазина
    document.getElementById('create-shop-form').addEventListener('submit', createShop);
    // Привязываем событие submit формы создания товара
    document.getElementById('create-product-form').addEventListener('submit', createProduct);
};



// Вызываем функцию для получения списка магазинов при загрузке страницы
window.onload = function () {
    fetchShopList();
};
async function login() {
    const response = await fetch('/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            username: 'yourUsername',
            password: 'yourPassword'
        })
    });

    if (response.ok) {
        console.log('Logged in successfully');
    } else {
        console.error('Failed to log in');
    }
}

async function logout() {
    const response = await fetch('/logout', {
        method: 'POST'
    });

    if (response.ok) {
        console.log('Logged out successfully');
    } else {
        console.error('Failed to log out');
    }
}
