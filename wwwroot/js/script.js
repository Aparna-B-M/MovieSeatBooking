// ===========================================
// Movie Seat Booking System
// ===========================================

const showId = "02adf7bb-4608-4c67-8ead-4a83ff9da251";

let selectedSeats = [];
let holdExpiryTime = null;

// HTML Controls

const seatContainer = document.getElementById("seatContainer");

const availableCount = document.getElementById("availableCount");

const heldCount = document.getElementById("heldCount");

const bookedCount = document.getElementById("bookedCount");

const selectedSeatLabel = document.getElementById("selectedSeat");

const countdown = document.getElementById("countdown");

const toast = document.getElementById("toast");

// ===========================================
// Page Load
// ===========================================

window.onload = () => {

    loadSeats();

    loadAvailability();

    setInterval(() => {

        loadSeats();

        loadAvailability();

    }, 5000);

    setInterval(updateCountdown,1000);

};
// ===========================================
// Toast Notification
// ===========================================

function showToast(message,success=true){

    toast.innerHTML = message;

    toast.style.display="block";

    toast.style.background=
        success
        ? "#16a34a"
        : "#dc2626";

    setTimeout(()=>{

        toast.style.display="none";

    },3000);

}
// ===========================================
// Load Seats
// ===========================================

async function loadSeats(){

    const response =
        await fetch(`/api/Booking/seats/${showId}`);

    const seats =
        await response.json();

    seatContainer.innerHTML="";

    seats.forEach(seat=>{

        const div=
            document.createElement("div");

        div.classList.add("seat");

        div.innerHTML=seat.seatNumber;
        if (selectedSeats.some(s => s.seatNumber === seat.seatNumber)) {
    div.classList.add("selected");
}

        switch(seat.status){

            case "AVAILABLE":

                div.classList.add("available");

                break;

            case "HELD":

                div.classList.add("heldSeat");

                break;

            case "BOOKED":

                div.classList.add("bookedSeat");

                break;

        }

        div.onclick=(e)=>{

            selectSeat(seat,e);

        };

        seatContainer.appendChild(div);

    });

}
// ===========================================
// Select Seat
// ===========================================

function selectSeat(seat, e) {
     console.log(selectedSeats.map(s => s.seatNumber));
    if (seat.status !== "AVAILABLE") {

        showToast("Seat is not available.", false);

        return;
    }

    const index = selectedSeats.findIndex(s => s.seatNumber === seat.seatNumber);

    if (index >= 0) {

        // Unselect
        selectedSeats.splice(index, 1);
        e.target.classList.remove("selected");

    } else {

        // Select
        selectedSeats.push(seat);
        e.target.classList.add("selected");
    }

    if (selectedSeats.length === 0) {

        selectedSeatLabel.innerHTML = "None";

    } else {

        selectedSeatLabel.innerHTML =
            selectedSeats.map(s => s.seatNumber).join(", ");
    }
    console.log(selectedSeats.map(s => s.seatNumber));
}
// ===========================================
// Dashboard
// ===========================================

async function loadAvailability(){

    const response=
        await fetch(`/api/Booking/availability/${showId}`);

    const result=
        await response.json();

    availableCount.innerHTML=
        result.available;

    heldCount.innerHTML=
        result.held;

    bookedCount.innerHTML=
        result.booked;

}
// ===========================================
// Countdown
// ===========================================

function updateCountdown(){

    if(holdExpiryTime==null){

        countdown.innerHTML="--:--";

        return;

    }

    let seconds=
        Math.floor(
            (holdExpiryTime-new Date())/1000);

    if(seconds<=0){

        countdown.innerHTML="Expired";

        holdExpiryTime=null;

        return;

    }

    const minutes=
        Math.floor(seconds/60);

    seconds=seconds%60;

    countdown.innerHTML=

        `${minutes}:${seconds
        .toString()
        .padStart(2,'0')}`;

}
// ===========================================
// Hold Seat
// ===========================================

document.getElementById("holdBtn").addEventListener("click", holdSeat);

async function holdSeat() {

    if (selectedSeats.length === 0) {

    showToast("Please select at least one seat.", false);

    return;
}

    try {

        const response = await fetch("/api/Booking/hold", {
    method: "POST",
    headers: {
        "Content-Type": "application/json"
    },
    body: JSON.stringify({
        showId: showId,
         seatNumbers: selectedSeats.map(s => s.seatNumber),
        idempotencyKey: ""
    })
});

        if (!response.ok) {

            const message = await response.text();

            showToast(message, false);

            return;
        }

        const result = await response.json();

        holdExpiryTime = new Date(result.expiresAt);

        showToast("Seat held successfully.");

        loadSeats();
        loadAvailability();

    }
    catch {

        showToast("Server error.", false);

    }

}
// ===========================================
// Confirm Booking
// ===========================================

document.getElementById("confirmBtn").addEventListener("click", confirmBooking);

async function confirmBooking() {

    if (selectedSeats.length === 0){

        showToast("Please select a seat.", false);

        return;

    }

    try {

        const idempotencyKey = crypto.randomUUID();

        const response = await fetch("/api/Booking/confirm", {
    method: "POST",
    headers: {
        "Content-Type": "application/json"
    },
    body: JSON.stringify({
        showId: showId,
         seatNumbers: selectedSeats.map(s => s.seatNumber),
        idempotencyKey: idempotencyKey
    })
});

        if (!response.ok) {

            const message = await response.text();

            showToast(message, false);

            return;

        }

        showToast("Booking confirmed!");

        selectedSeats = [];

        holdExpiryTime = null;

        selectedSeatLabel.innerHTML = "None";
        document
    .querySelectorAll(".seat")
    .forEach(x => x.classList.remove("selected"));

        loadSeats();

        loadAvailability();

    }
    catch {

        showToast("Server error.", false);

    }

}