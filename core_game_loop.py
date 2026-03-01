"""Core gameplay loop prototype for a warehouse management game.

Game loop:
Create warehouse -> Hire employees -> Buy equipment ->
Take delivery -> Storage -> Pickup -> Shipment -> Profit -> Expansion -> Loop
"""

from __future__ import annotations

from dataclasses import dataclass, field


@dataclass
class WarehouseGame:
    cash: float = 50_000.0
    warehouse_capacity: int = 100
    employees: int = 0
    equipment_level: int = 0
    inventory: int = 0
    pending_pickups: int = 0
    completed_shipments: int = 0
    day: int = 0
    log: list[str] = field(default_factory=list)

    EMPLOYEE_COST: float = 2_000.0
    EQUIPMENT_COST: float = 5_000.0
    STORAGE_FEE_PER_UNIT: float = 8.0
    SHIPMENT_REVENUE_PER_UNIT: float = 20.0
    EXPANSION_COST: float = 25_000.0
    EXPANSION_CAPACITY_GAIN: int = 50

    def run_day(self) -> None:
        """Runs one full cycle of the core game loop."""
        self.day += 1
        self.log.append(f"--- Day {self.day} ---")

        self.create_warehouse()
        self.hire_employees()
        self.buy_equipment()
        self.take_delivery()
        self.storage()
        self.pickup()
        self.shipment()
        self.profit()
        self.expansion()

    def create_warehouse(self) -> None:
        """Bootstraps the warehouse if this is a fresh game."""
        if self.day == 1:
            self.log.append(
                f"Warehouse created with capacity {self.warehouse_capacity}."
            )

    def hire_employees(self) -> None:
        """Hire enough employees to process inbound and outbound tasks."""
        target = max(3, self.warehouse_capacity // 50)
        can_hire = int(self.cash // self.EMPLOYEE_COST)
        hired = max(0, min(target - self.employees, can_hire))

        if hired:
            self.employees += hired
            self.cash -= hired * self.EMPLOYEE_COST
            self.log.append(f"Hired {hired} employees (total: {self.employees}).")

    def buy_equipment(self) -> None:
        """Increase equipment level to improve throughput."""
        desired_level = max(1, self.warehouse_capacity // 100)
        affordable = int(self.cash // self.EQUIPMENT_COST)
        bought = max(0, min(desired_level - self.equipment_level, affordable))

        if bought:
            self.equipment_level += bought
            self.cash -= bought * self.EQUIPMENT_COST
            self.log.append(
                f"Bought {bought} equipment upgrade(s) (level: {self.equipment_level})."
            )

    def take_delivery(self) -> None:
        """Inbound goods arrive from suppliers."""
        inbound = 10 + (self.equipment_level * 5)
        free_space = max(0, self.warehouse_capacity - self.inventory)
        accepted = min(inbound, free_space)
        self.inventory += accepted
        self.log.append(f"Delivery received: {accepted} units.")

    def storage(self) -> None:
        """Stored inventory incurs a handling fee and creates future demand."""
        storage_fee = self.inventory * self.STORAGE_FEE_PER_UNIT * 0.1
        self.cash -= storage_fee
        self.pending_pickups += max(1, self.inventory // 4)
        self.log.append(
            f"Storage handled for {self.inventory} units (fee: ${storage_fee:,.0f})."
        )

    def pickup(self) -> None:
        """Customers schedule pickups based on employee throughput."""
        throughput = (self.employees * 2) + (self.equipment_level * 3)
        ready_to_ship = min(self.pending_pickups, throughput, self.inventory)
        self.pending_pickups -= ready_to_ship
        self.inventory -= ready_to_ship
        self.completed_shipments = ready_to_ship
        self.log.append(f"Pickup prepared: {ready_to_ship} units.")

    def shipment(self) -> None:
        """Pickup loads are shipped and revenue is booked."""
        if self.completed_shipments > 0:
            revenue = self.completed_shipments * self.SHIPMENT_REVENUE_PER_UNIT
            self.cash += revenue
            self.log.append(
                f"Shipment sent: {self.completed_shipments} units (revenue: ${revenue:,.0f})."
            )

    def profit(self) -> None:
        """Checks profitability after current cycle."""
        status = "profit" if self.cash > 0 else "loss"
        self.log.append(f"End-of-day cash: ${self.cash:,.0f} ({status}).")

    def expansion(self) -> None:
        """Reinvest profit into warehouse expansion, then loop continues."""
        if self.cash >= self.EXPANSION_COST:
            self.cash -= self.EXPANSION_COST
            self.warehouse_capacity += self.EXPANSION_CAPACITY_GAIN
            self.log.append(
                f"Expansion complete: +{self.EXPANSION_CAPACITY_GAIN} capacity "
                f"(total: {self.warehouse_capacity})."
            )


def simulate(days: int = 5) -> list[str]:
    game = WarehouseGame()
    for _ in range(days):
        game.run_day()
    return game.log


if __name__ == "__main__":
    for line in simulate(7):
        print(line)
